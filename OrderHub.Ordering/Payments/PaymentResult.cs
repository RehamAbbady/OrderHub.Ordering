using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderHub.Ordering.Payments
{
    public sealed record PaymentResult(bool Succeeded, string? IntentId);

}
