using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderHub.Ordering.Pricing
{
    public sealed class LinePricer : ILinePricer
    {
        private const decimal GoldMultiplier = 0.85m;
        private const decimal SilverMultiplier = 0.92m;
        private const decimal ShortEmbroiderySurcharge = 4.50m;
        private const decimal LongEmbroiderySurcharge = 8.00m;
        private const int ShortEmbroideryMaxLength = 3;

        public decimal UnitPrice(decimal basePrice, string? tierCode, string? embroidery)
        {
            var price = basePrice * TierMultiplier(tierCode);

            if (!string.IsNullOrEmpty(embroidery))
            {
                price += embroidery.Length <= ShortEmbroideryMaxLength
                    ? ShortEmbroiderySurcharge
                    : LongEmbroiderySurcharge;
            }

            return Math.Round(price, 2, MidpointRounding.AwayFromZero);
        }

        public decimal LineTotal(decimal basePrice, string? tierCode, string? embroidery, int quantity)
            => UnitPrice(basePrice, tierCode, embroidery) * quantity;

        private static decimal TierMultiplier(string? tierCode) => tierCode switch
        {
            "GOLD" => GoldMultiplier,
            "SILVER" => SilverMultiplier,
            _ => 1m
        };
    }
}
