using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderHub.Ordering.Pricing
{

    public interface ILinePricer
    {
        decimal UnitPrice(decimal basePrice, string? tierCode, string? embroidery);
        decimal LineTotal(decimal basePrice, string? tierCode, string? embroidery, int quantity);
    }
}
