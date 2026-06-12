using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.Common;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderHub.Ordering.Infrastructure
{
    public sealed class SqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(IOptions<OrderingOptions> options)
            => _connectionString = options.Value.ConnectionString;

        public async Task<DbConnection> OpenAsync(CancellationToken ct)
        {
            var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            return conn;
        }
    }
}
