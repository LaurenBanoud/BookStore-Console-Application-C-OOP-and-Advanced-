using System.Globalization;

namespace BookStoreApp.Extensions;

/// <summary>
/// Helper methods bolted onto decimal, used across the pricing/reporting code.
/// </summary>
public static class NumberExtensions
{
    private static readonly CultureInfo UsCulture = CultureInfo.GetCultureInfo("en-US");

    public static string ToCurrency(this decimal value) => value.ToString("C", UsCulture);

    public static decimal ApplyPercentageDiscount(this decimal price, decimal percent) =>
        Math.Round(price - (price * percent / 100m), 2, MidpointRounding.AwayFromZero);

    public static bool IsBetween(this decimal value, decimal min, decimal max) =>
        value >= min && value <= max;
}
