# OrderHub.Ordering

.NET 8 reimplementation of Brindleford & Co's order-processing core, extracted
from the legacy ASP.NET WebForms `OrderProcessor`. Handles school tier pricing,
embroidery surcharges, inventory checks, payment-intent creation, and
confirmation queueing.

## Projects

- `OrderHub.Ordering` —> domain core (orders, pricing, payments, confirmations)
- `OrderHub.Ordering.Infrastructure` —> Dapper/SQL access, HTTP payment gateway, DI wiring
- `OrderHub.Ordering.Tests` —> xUnit tests

## Build & test

```bash
dotnet build
dotnet test
```

## Migration

This component is being introduced via a strangler-fig migration off the legacy
1800-line `OrderProcessor`, with zero downtime across the peak season. See
[MIGRATION.md](MIGRATION.md) for the approach, the legacy adapter that keeps
existing Razor/ASMX callers working, and the parity risk for leadership.
