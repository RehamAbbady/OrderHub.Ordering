using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderHub.Ordering.Abstractions
{
    public interface IInventory
    {
        Task<IReadOnlyDictionary<string, int>> GetStockAsync(IReadOnlyCollection<string> skus, CancellationToken ct);
    }

}
