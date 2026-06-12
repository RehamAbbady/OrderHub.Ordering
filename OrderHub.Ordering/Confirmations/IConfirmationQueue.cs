using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderHub.Ordering.Confirmations
{
    public interface IConfirmationQueue
    {
        Task EnqueueAsync(OrderConfirmation confirmation, CancellationToken ct);
    }
}
