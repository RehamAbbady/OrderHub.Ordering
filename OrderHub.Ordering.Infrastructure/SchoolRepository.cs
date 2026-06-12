using Dapper;
using OrderHub.Ordering.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderHub.Ordering.Infrastructure
{
    public sealed class SchoolRepository : ISchoolRepository
    {
        private readonly IDbConnectionFactory _connections;

        public SchoolRepository(IDbConnectionFactory connections) => _connections = connections;

        public async Task<string?> GetTierCodeAsync(int schoolId, CancellationToken ct)
        {
            await using var conn = await _connections.OpenAsync(ct);
            return await conn.QuerySingleOrDefaultAsync<string?>(
                new CommandDefinition(
                    "SELECT TierCode FROM Schools WHERE Id = @schoolId",
                    new { schoolId },
                    cancellationToken: ct));
        }
    }
}
