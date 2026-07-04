using System.Net.Mail;

namespace BookStoreApp.Extensions;

/// <summary>
/// Small helper methods bolted onto the built-in string type (requirement 11:
/// "add helper methods to existing .NET types where they make your code
/// cleaner"). Keeping validation/formatting logic here avoids repeating it
/// throughout the domain and UI layers.
/// </summary>
public static class StringExtensions
{
    public static bool IsValidEmail(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        try
        {
            var address = new MailAddress(value);
            return address.Address.Equals(value, StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public static string ToTitleCase(this string value) =>
        System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(value.ToLowerInvariant());

    public static string Truncate(this string value, int maxLength) =>
        value.Length <= maxLength ? value : string.Concat(value.AsSpan(0, maxLength), "...");
}
