# OrderHub - Decomposing OrderProcessor (Q4)

The .NET 8 `OrderProcessor` does validation, pricing, inventory, payment, and
confirmation in one method on one SQL connection. Splitting it means giving up
that single in-process transaction, so the real work here is deciding where the
boundaries go and how consistency holds without it.

## 1. Service boundaries

| Service | Owns | Responsibility | Contract |
|---|---|---|---|
| **Order** | Order aggregate and its state machine (Pending, Reserved, Paid, Confirmed, Failed) | Entry point. Runs the saga, owns order status and lifecycle | Receives `PlaceOrder` (sync command from the web). Publishes `OrderConfirmed` / `OrderFailed` events |
| **Catalog & Pricing** | Products, base prices, school tiers, pricing rules | Validates SKUs, applies tier discount and embroidery surcharge, returns the amount to charge | Sync request/response (`PriceOrder`). Deterministic and read-only |
| **Inventory** | Stock levels, reservations | Reserves stock against an orderId, releases on cancel or timeout | Sync `ReserveStock`, idempotent on orderId. Emits `StockReserved` / `ReservationRejected` |
| **Payment** | Payment intents, provider integration, webhooks | Creates an intent keyed on orderId, captures provider webhooks. Source of truth for whether money moved | Sync `CreateIntent` returns the intent. Async `PaymentSucceeded` / `PaymentFailed` from the webhook |
| **Notification** | Confirmation emails, dedupe ledger | Consumes `OrderConfirmed`, sends one email per order, retries | Async event consumer. At-least-once with dedupe on (orderId, type) |

I kept pricing inside the Catalog service rather than splitting it out. Pricing is a
pure function over catalogue data (tier and base price), so a separate service would
just add a network hop for no isolation benefit. The rules live next to the data they
read.

### Transaction boundaries

There is no distributed transaction and no two-phase commit. Each service commits only
its own local database transaction. The Order service holds consistency through an
orchestration saga, not a shared transaction.

It drives the steps in order, and every step that changes external state has a
compensating action:

1. Order writes the order as Pending in a local transaction.
2. Calls Catalog & Pricing to validate and price. A bad SKU or unknown school fails
   fast, the order is marked Failed, nothing else is touched.
3. Calls Inventory to reserve, keyed on orderId. Insufficient stock fails fast with
   nothing to compensate.
4. Calls Payment to create an intent keyed on orderId. On success the order moves to
   Confirmed.
5. Publishes `OrderConfirmed`. Notification emails the parent.

An order reaches Confirmed on exactly two facts: inventory is committed and payment
succeeded. Any failure after a reservation exists runs compensation, release the
reservation, and cancel or refund the intent if one was created. The confirmation
email sits outside this boundary on purpose, covered below.

## 2. Confirmation flow (~150 words)

From Submit, the web posts `PlaceOrder` synchronously and shows a pending state while
the saga prices, reserves, and creates the intent. Intent creation returns
synchronously in the common path, so the admin sees "order confirmed" the moment the
Order service writes Confirmed, not after the email sends. The provider webhook is the
reconciliation path for late or async results.

Broker is Azure Service Bus, chosen for managed dead-lettering, duplicate detection,
and per-order sessions on a .NET/Azure stack. Notification consumes `OrderConfirmed`
with competing consumers. Retries use capped exponential backoff with jitter, then
dead-letter for replay. Every message is keyed on orderId, and Notification dedupes on
(orderId, type), so redelivery never sends two emails.

If payment succeeds but the email fails, the order stays Confirmed. The email retries
and finally dead-letters for reprocessing. A bounced email never reverses a real
charge.

## 3. Runbook: late payment webhook on a Failed order (~150 words)

**Trigger:** a `PaymentSucceeded` webhook arrives for an order already in Failed state.
The payment timed out earlier, the saga marked it Failed and released the reservation.

**What is happening:** the provider captured money after we gave up. Real charge, no
fulfilled order, stock already released. Do not flip straight to Confirmed. The stock
that backed this order may now be sold to someone else.

**Steps:**
1. Look up the order by orderId from the webhook. Confirm state is Failed and exactly
   one capture exists (idempotency should guarantee one).
2. Ask Inventory whether the reservation can be remade right now.
3. If yes, re-reserve, move to Confirmed, publish `OrderConfirmed` so the parent gets
   their email, and note the manual recovery on the order.
4. If no, refund the capture through Payment and send the "could not fulfil" notice.

**Escalate** to payments on-call if the refund fails or two captures exist.
