using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderHub.Ordering.Infrastructure
{
    public sealed class Inventory : IInventory
    {
        private readonly IDbConnectionFactory _connections;

        public Inventory(IDbConnectionFactory connections) => _connections = connections;

        public async Task<IReadOnlyDictionary<string, int>> GetStockAsync(
            IReadOnlyCollection<string> skus, CancellationToken ct)
        {
            if (skus.Count == 0)
                return new Dictionary<string, int>();

            await using var conn = await _connections.OpenAsync(ct);
            var rows = await conn.QueryAsync<(string Sku, int Qty)>(
                new CommandDefinition(
                    "SELECT Sku, Qty FROM Stock WHERE Sku IN @skus",
                    new { skus },
                    cancellationToken: ct));

            return rows.ToDictionary(r => r.Sku, r => r.Qty);
        }
    }

}
