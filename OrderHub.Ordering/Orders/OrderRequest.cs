using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderHub.Ordering.Orders
{
    public sealed record OrderRequest(int SchoolId, IReadOnlyList<OrderLine> Lines, string ParentEmail);

}
