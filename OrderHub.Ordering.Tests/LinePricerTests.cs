using OrderHub.Ordering.Pricing;
using Xunit;

namespace OrderHub.Ordering.Tests;

public class LinePricerTests
{
    private readonly LinePricer _pricer = new();

    [Theory]
    [InlineData(20.00, "GOLD", "AB", 3, 64.50)]
    [InlineData(50.00, "SILVER", "LONGNAME", 2, 108.00)]
    [InlineData(19.99, "GOLD", null, 1, 16.99)]
    [InlineData(10.00, "BRONZE", "", 1, 10.00)]
    [InlineData(20.00, "GOLD", "ABC", 2, 43.00)]
    public void LineTotal_combines_tier_then_embroidery_then_quantity(
        decimal basePrice, string tierCode, string? embroidery, int quantity, decimal expected)
    {
        var actual = _pricer.LineTotal(basePrice, tierCode, embroidery, quantity);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("GOLD", 85.00)]
    [InlineData("SILVER", 92.00)]
    [InlineData("BRONZE", 100.00)]
    [InlineData(null, 100.00)]
    public void Tier_discount_applies_only_for_known_tiers(string? tierCode, decimal expected)
    {
        var actual = _pricer.UnitPrice(100.00m, tierCode, null);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(null, 10.00)]
    [InlineData("", 10.00)]
    [InlineData("A", 14.50)]
    [InlineData("ABC", 14.50)]
    [InlineData("ABCD", 18.00)]
    [InlineData("ABCDEFGH", 18.00)]
    public void Embroidery_surcharge_changes_at_three_character_boundary(string? embroidery, decimal expected)
    {
        var actual = _pricer.UnitPrice(10.00m, "BRONZE", embroidery);

        Assert.Equal(expected, actual);
    }
}