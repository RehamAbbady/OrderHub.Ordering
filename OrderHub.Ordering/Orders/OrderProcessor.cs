using OrderHub.Ordering.Abstractions;
using OrderHub.Ordering.Confirmations;
using OrderHub.Ordering.Payments;
using OrderHub.Ordering.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderHub.Ordering.Orders
{
    public sealed class OrderProcessor : IOrderProcessor
    {
        private readonly ISchoolRepository _schools;
        private readonly IProductCatalog _catalog;
        private readonly IInventory _inventory;
        private readonly IPaymentGateway _payments;
        private readonly IConfirmationQueue _confirmations;
        private readonly ILinePricer _pricer;

        public OrderProcessor(
            ISchoolRepository schools,
            IProductCatalog catalog,
            IInventory inventory,
            IPaymentGateway payments,
            IConfirmationQueue confirmations,
            ILinePricer pricer)
        {
            _schools = schools;
            _catalog = catalog;
            _inventory = inventory;
            _payments = payments;
            _confirmations = confirmations;
            _pricer = pricer;
        }

        public async Task<OrderResult> ProcessAsync(OrderRequest request, CancellationToken ct = default)
        {
            var tier = await _schools.GetTierCodeAsync(request.SchoolId, ct);
            if (tier is null)
                return OrderResult.Failure(OrderFailureReason.SchoolNotFound);

            var skus = request.Lines.Select(l => l.Sku).Distinct().ToArray();
            var prices = await _catalog.GetBasePricesAsync(skus, ct);
            var stock = await _inventory.GetStockAsync(skus, ct);

            foreach (var line in request.Lines)
            {
                if (!prices.ContainsKey(line.Sku))
                    return OrderResult.Failure(OrderFailureReason.ProductNotFound, line.Sku);

                if (!stock.TryGetValue(line.Sku, out var available) || available < line.Quantity)
                    return OrderResult.Failure(OrderFailureReason.OutOfStock, line.Sku);
            }

            var subtotal = request.Lines.Sum(line =>
                _pricer.LineTotal(prices[line.Sku], tier, line.Embroidery, line.Quantity));

            var payment = await _payments.CreateIntentAsync(subtotal, request.ParentEmail, ct);
            if (!payment.Succeeded || payment.IntentId is null)
                return OrderResult.Failure(OrderFailureReason.PaymentDeclined);

            await _confirmations.EnqueueAsync(
                new OrderConfirmation(request.ParentEmail, subtotal, payment.IntentId), ct);

            return OrderResult.Success(subtotal, payment.IntentId);
        }
    }
}
