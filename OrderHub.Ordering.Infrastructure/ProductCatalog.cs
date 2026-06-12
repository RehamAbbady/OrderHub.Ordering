using Dapper;
using OrderHub.Ordering.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderHub.Ordering.Infrastructure
{

    public sealed class ProductCatalog : IProductCatalog
    {
        private readonly IDbConnectionFactory _connections;

        public ProductCatalog(IDbConnectionFactory connections) => _connections = connections;

        public async Task<IReadOnlyDictionary<string, decimal>> GetBasePricesAsync(
            IReadOnlyCollection<string> skus, CancellationToken ct)
        {
            if (skus.Count == 0)
                return new Dictionary<string, decimal>();

            await using var conn = await _connections.OpenAsync(ct);
            var rows = await conn.QueryAsync<(string Sku, decimal BasePrice)>(
                new CommandDefinition(
                    "SELECT Sku, BasePrice FROM Products WHERE Sku IN @skus",
                    new { skus },
                    cancellationToken: ct));

            return rows.ToDictionary(r => r.Sku, r => r.BasePrice);
        }
    }

}
