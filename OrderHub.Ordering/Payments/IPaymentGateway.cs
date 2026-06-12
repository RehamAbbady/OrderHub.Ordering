using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderHub.Ordering.Payments
{
    public interface IPaymentGateway
    {
        Task<PaymentResult> CreateIntentAsync(decimal amount, string parentEmail, CancellationToken ct);
    }
}
