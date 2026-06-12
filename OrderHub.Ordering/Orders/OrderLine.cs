using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderHub.Ordering.Orders
{
    public sealed record OrderLine(string Sku, int Quantity, string? Embroidery = null);

}
