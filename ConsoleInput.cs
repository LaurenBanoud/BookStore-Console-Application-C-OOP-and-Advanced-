using BookStoreApp.Extensions;

namespace BookStoreApp.UI;

/// <summary>
/// Every read here loops until it gets a valid value, so the menu can never
/// crash from a malformed console input (requirement 4).
/// </summary>
public static class ConsoleInput
{
    public static string ReadRequiredString(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
                return input.Trim();

            Console.WriteLine("  x This field cannot be empty. Please try again.");
        }
    }

    public static decimal ReadPositiveDecimal(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            if (decimal.TryParse(input, out var value) && value > 0)
                return value;

            Console.WriteLine("  x Please enter a number greater than zero.");
        }
    }

    public static decimal ReadNonNegativeDecimal(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            if (decimal.TryParse(input, out var value) && value >= 0)
                return value;

            Console.WriteLine("  x Please enter a number that is zero or greater.");
        }
    }

    public static double ReadPositiveDouble(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            if (double.TryParse(input, out var value) && value > 0)
                return value;

            Console.WriteLine("  x Please enter a number greater than zero.");
        }
    }

    public static int ReadPositiveInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            if (int.TryParse(input, out var value) && value > 0)
                return value;

            Console.WriteLine("  x Please enter a whole number greater than zero.");
        }
    }

    public static int ReadNonNegativeInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            if (int.TryParse(input, out var value) && value >= 0)
                return value;

            Console.WriteLine("  x Please enter a whole number that is zero or greater.");
        }
    }

    public static string ReadEmail(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            if (input.IsValidEmail())
                return input!.Trim();

            Console.WriteLine("  x Please enter a valid email address (e.g. name@example.com).");
        }
    }

    public static int ReadMenuChoice(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            if (int.TryParse(input, out var value) && value >= min && value <= max)
                return value;

            Console.WriteLine($"  x Please enter a number between {min} and {max}.");
        }
    }

    public static bool ReadYesNo(string prompt)
    {
        while (true)
        {
            Console.Write($"{prompt} (y/n): ");
            var input = Console.ReadLine()?.Trim().ToLowerInvariant();
            if (input is "y" or "yes")
                return true;
            if (input is "n" or "no")
                return false;

            Console.WriteLine("  x Please answer 'y' or 'n'.");
        }
    }

    public static Guid ReadGuid(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            if (Guid.TryParse(input, out var value))
                return value;

            Console.WriteLine("  x Please enter a valid ID (copy it exactly from the list above).");
        }
    }
}
