using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderHub.Ordering.Abstractions
{
    public interface ISchoolRepository
    {
        Task<string?> GetTierCodeAsync(int schoolId, CancellationToken ct);
    }
}
