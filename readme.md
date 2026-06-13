# OrderHub.Ordering

.NET 8 reimplementation of Brindleford & Co's order-processing core, extracted
from the legacy ASP.NET WebForms `OrderProcessor`. Handles school tier pricing,
embroidery surcharges, inventory checks, payment-intent creation, and
confirmation queueing.

## Projects

- `OrderHub.Ordering` —> domain core (orders, pricing, payments, confirmations)
- `OrderHub.Ordering.Infrastructure` —> Dapper/SQL access, HTTP payment gateway, DI wiring
- `OrderHub.Ordering.Tests` —> xUnit tests
- `OrderHub.Web`-> ASP.NET Core Razor Pages host, including the rebuilt confirm-order page


## Build & test

```bash
dotnet build
dotnet test
```
## Running the web app

```bash
dotnet run --project OrderHub.Web
```
The root URL redirects to `/Orders/ConfirmOrder/1`.

The confirm page and its live subtotal run entirely in-memory on the GET path, so
they work with no database. The order draft is served by an in-memory store and the
confirmation queue is a logging stub, both registered in the web host so the Q1
projects stay untouched.

Clicking Confirm hands off to the Q1 `OrderProcessor`, which needs a real database
and payment provider. That backend is intentionally left unwired in this submission,
so a Confirm surfaces a "not available in this demo" message rather than completing.
The page rewrite and the no-reload subtotal are what this part demonstrates, and both
run without the backend.

## Migration

This component is being introduced via a strangler-fig migration off the legacy
1800-line `OrderProcessor`, with zero downtime across the peak season. See
[MIGRATION.md](MIGRATION.md) for the approach, the legacy adapter that keeps
existing Razor/ASMX callers working, and the parity risk for leadership.
