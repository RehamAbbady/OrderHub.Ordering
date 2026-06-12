using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderHub.Ordering.Abstractions
{
    public interface IProductCatalog
    {
        Task<IReadOnlyDictionary<string, decimal>> GetBasePricesAsync(IReadOnlyCollection<string> skus, CancellationToken ct);
    }

}
