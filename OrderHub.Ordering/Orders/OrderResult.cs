using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderHub.Ordering.Orders
{
    public sealed record OrderResult
    {
        private OrderResult() { }

        public bool Succeeded { get; private init; }
        public decimal Subtotal { get; private init; }
        public string? PaymentIntentId { get; private init; }
        public OrderFailureReason? Reason { get; private init; }
        public string? Detail { get; private init; }

        public static OrderResult Success(decimal subtotal, string paymentIntentId) => new()
        {
            Succeeded = true,
            Subtotal = subtotal,
            PaymentIntentId = paymentIntentId
        };

        public static OrderResult Failure(OrderFailureReason reason, string? detail = null) => new()
        {
            Succeeded = false,
            Reason = reason,
            Detail = detail
        };
    }

}
