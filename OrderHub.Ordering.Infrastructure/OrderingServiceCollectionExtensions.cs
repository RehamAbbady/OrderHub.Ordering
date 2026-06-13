using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Http;
using OrderHub.Ordering.Abstractions;
using OrderHub.Ordering.Orders;
using OrderHub.Ordering.Payments;
using OrderHub.Ordering.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderHub.Ordering.Infrastructure
{
    public static class OrderingServiceCollectionExtensions
    {
        public static IServiceCollection AddOrderProcessing(
            this IServiceCollection services,
            Action<OrderingOptions> configure)
        {
            services.Configure(configure);

            services.AddSingleton<ILinePricer, LinePricer>();
            services.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();
            services.AddScoped<ISchoolRepository, SchoolRepository>();
            services.AddScoped<IProductCatalog, ProductCatalog>();
            services.AddScoped<IInventory, Inventory>();
            services.AddScoped<IOrderProcessor, OrderProcessor>();

            services.AddOptions<OrderingOptions>()
                .Configure(configure)
                .Validate(o => !string.IsNullOrWhiteSpace(o.ConnectionString),
                    "OrderingOptions.ConnectionString is required")
                .Validate(o => Uri.IsWellFormedUriString(o.PaymentBaseUrl, UriKind.Absolute),
                    "OrderingOptions.PaymentBaseUrl must be an absolute URI")
                .ValidateOnStart();

            services.AddHttpClient<IPaymentGateway, HttpPaymentGateway>()
                .ConfigureHttpClient((sp, client) =>
                {
                    var url = sp.GetRequiredService<IOptions<OrderingOptions>>().Value.PaymentBaseUrl;
                    client.BaseAddress = new Uri(url);
                });

            return services;
        }
    }

}
