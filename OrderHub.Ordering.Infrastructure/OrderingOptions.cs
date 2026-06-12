using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderHub.Ordering.Infrastructure
{
    public sealed class OrderingOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string PaymentBaseUrl { get; set; } = string.Empty;
    }
}
