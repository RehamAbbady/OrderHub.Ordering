using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderHub.Ordering.Infrastructure
{
    public interface IDbConnectionFactory
    {
        Task<DbConnection> OpenAsync(CancellationToken ct);
    }
}