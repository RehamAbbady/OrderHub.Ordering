# MIGRATION.md

## The approach

The main constraint is that OrderHub must remain available during its busiest periods. Because of this, the migration will follow a strangler pattern rather than a complete rewrite. New functionality will be built alongside the existing system, and the legacy code will only be removed after the new implementation has been thoroughly tested and proven reliable.

The clean implementation sits in two projects. OrderHub.Ordering is the domain: order flow, pricing, and the interfaces for its dependencies. It has no database or socket access. OrderHub.Ordering.Infrastructure contains the implementations: Dapper repositories, the connection factory, and the HTTP gateway. It references the domain, but the domain does not reference Dapper or SqlClient, so any accidental use of them in domain code will not compile.
It also keeps things safe, the domain assembly runs under the old 4.7 host today and the .NET 8 host later, so nothing is ported twice.

For the cutover, put every call site behind `IOrderProcessor` resolved through DI, so the Razor pages and ASMX services stop newing up the concrete class. That ships first, no behaviour change. Then register both implementations and pick between them with a flag, starting with one canary school. Run it in shadow against live traffic first, logging only where it disagrees with the old path; after a clean peak season, delete the legacy class and the flag.

## Tests

Pricing and the orchestrator are covered, including the guarantees that matter most: a bad order is never charged, and a declined payment never sends a confirmation. The repositories and gateway still run on mocks; the real SQL and HTTP parsing need integration tests before we trust them. One gap to log: duplicate SKUs are checked per line, not by combined quantity, so an order can oversell, which needs a reject-or-merge decision before go-live.

## One thing I noticed

Modernising surfaced two quiet behaviours the new version cleans up. The old code keeps full decimal precision on a discount and sums only at the end; the new version rounds each line to the penny. It also swallows email failures, so some confirmations silently never send; the new version surfaces them. Both are improvements, but they nudge visible numbers: a subtotal can land a penny off the old logic, and more confirmations go out than before. Worth agreeing the rounding rule and an old-against-new reconciliation before the first cutover, not after.
