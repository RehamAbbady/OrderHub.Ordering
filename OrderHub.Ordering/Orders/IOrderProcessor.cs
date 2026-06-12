using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderHub.Ordering.Orders
{
    public interface IOrderProcessor
    {
        Task<OrderResult> ProcessAsync(OrderRequest request, CancellationToken ct = default);
    }
}
