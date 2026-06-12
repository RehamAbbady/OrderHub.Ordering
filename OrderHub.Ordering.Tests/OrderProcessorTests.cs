using Moq;
using OrderHub.Ordering.Abstractions;
using OrderHub.Ordering.Confirmations;
using OrderHub.Ordering.Orders;
using OrderHub.Ordering.Payments;
using OrderHub.Ordering.Pricing;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OrderHub.Ordering.Tests;

public class OrderProcessorTests
{
    private readonly Mock<ISchoolRepository> _schools = new();
    private readonly Mock<IProductCatalog> _catalog = new();
    private readonly Mock<IInventory> _inventory = new();
    private readonly Mock<IPaymentGateway> _payments = new();
    private readonly Mock<IConfirmationQueue> _confirmations = new();
    private readonly ILinePricer _pricer = new LinePricer();

    private const string Email = "parent@example.com";

    public OrderProcessorTests()
    {
        _schools
            .Setup(s => s.GetTierCodeAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("GOLD");

        _catalog
            .Setup(c => c.GetBasePricesAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, decimal> { ["BLZ"] = 20.00m });

        _inventory
            .Setup(i => i.GetStockAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, int> { ["BLZ"] = 100 });

        _payments
            .Setup(p => p.CreateIntentAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentResult(true, "pi_123"));
    }

    private OrderProcessor BuildSut() => new(
        _schools.Object, _catalog.Object, _inventory.Object,
        _payments.Object, _confirmations.Object, _pricer);

    private static OrderRequest Request(params OrderLine[] lines) => new(1, lines, Email);

    private void VerifyNeverCharged() => _payments.Verify(
        p => p.CreateIntentAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
        Times.Never);

    [Fact]
    public async Task Unknown_school_fails_and_never_charges()
    {
        _schools
            .Setup(s => s.GetTierCodeAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var result = await BuildSut().ProcessAsync(Request(new OrderLine("BLZ", 1)));

        Assert.False(result.Succeeded);
        Assert.Equal(OrderFailureReason.SchoolNotFound, result.Reason);
        VerifyNeverCharged();
    }

    [Fact]
    public async Task Unknown_product_fails_with_sku_and_never_charges()
    {
        _catalog
            .Setup(c => c.GetBasePricesAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, decimal>());

        var result = await BuildSut().ProcessAsync(Request(new OrderLine("BLZ", 1)));

        Assert.False(result.Succeeded);
        Assert.Equal(OrderFailureReason.ProductNotFound, result.Reason);
        Assert.Equal("BLZ", result.Detail);
        VerifyNeverCharged();
    }

    [Fact]
    public async Task Insufficient_stock_fails_with_sku_and_never_charges()
    {
        _inventory
            .Setup(i => i.GetStockAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, int> { ["BLZ"] = 2 });

        var result = await BuildSut().ProcessAsync(Request(new OrderLine("BLZ", 5)));

        Assert.False(result.Succeeded);
        Assert.Equal(OrderFailureReason.OutOfStock, result.Reason);
        Assert.Equal("BLZ", result.Detail);
        VerifyNeverCharged();
    }

    [Fact]
    public async Task Any_out_of_stock_line_blocks_payment_for_the_whole_order()
    {
        _catalog
            .Setup(c => c.GetBasePricesAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, decimal> { ["BLZ"] = 20.00m, ["TIE"] = 8.00m });
        _inventory
            .Setup(i => i.GetStockAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, int> { ["BLZ"] = 100, ["TIE"] = 1 });

        var result = await BuildSut().ProcessAsync(Request(
            new OrderLine("BLZ", 1),
            new OrderLine("TIE", 5)));

        Assert.False(result.Succeeded);
        Assert.Equal(OrderFailureReason.OutOfStock, result.Reason);
        Assert.Equal("TIE", result.Detail);
        VerifyNeverCharged();
    }

    [Fact]
    public async Task Declined_payment_fails_and_enqueues_no_confirmation()
    {
        _payments
            .Setup(p => p.CreateIntentAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentResult(false, null));

        var result = await BuildSut().ProcessAsync(Request(new OrderLine("BLZ", 1)));

        Assert.False(result.Succeeded);
        Assert.Equal(OrderFailureReason.PaymentDeclined, result.Reason);
        _confirmations.Verify(
            c => c.EnqueueAsync(It.IsAny<OrderConfirmation>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Successful_order_charges_subtotal_and_enqueues_one_confirmation()
    {
        var result = await BuildSut().ProcessAsync(Request(new OrderLine("BLZ", 3)));

        Assert.True(result.Succeeded);
        Assert.Equal(51.00m, result.Subtotal);
        Assert.Equal("pi_123", result.PaymentIntentId);

        _payments.Verify(
            p => p.CreateIntentAsync(51.00m, Email, It.IsAny<CancellationToken>()),
            Times.Once);
        _confirmations.Verify(
            c => c.EnqueueAsync(
                It.Is<OrderConfirmation>(o =>
                    o.ParentEmail == Email &&
                    o.Subtotal == 51.00m &&
                    o.PaymentIntentId == "pi_123"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Subtotal_sums_every_line_with_tier_and_embroidery_applied()
    {
        _catalog
            .Setup(c => c.GetBasePricesAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, decimal> { ["BLZ"] = 20.00m, ["TIE"] = 8.00m });
        _inventory
            .Setup(i => i.GetStockAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, int> { ["BLZ"] = 100, ["TIE"] = 100 });

        var result = await BuildSut().ProcessAsync(Request(
            new OrderLine("BLZ", 1, "AB"),
            new OrderLine("TIE", 2)));

        Assert.True(result.Succeeded);
        Assert.Equal(35.10m, result.Subtotal);
    }
}