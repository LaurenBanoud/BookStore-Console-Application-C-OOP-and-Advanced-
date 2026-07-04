using BookStoreApp.Extensions;

namespace BookStoreApp.Validation;

/// <summary>
/// Central place for the invariants every domain object must satisfy.
/// Domain classes call these in their constructors so it is impossible
/// to end up with an invalid Book, Customer, or Purchase in memory.
/// </summary>
public static class Validator
{
    public static void EnsureNotEmpty(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException($"'{fieldName}' cannot be empty.");
    }

    public static void EnsurePositive(decimal value, string fieldName)
    {
        if (value <= 0)
            throw new ValidationException($"'{fieldName}' must be greater than zero.");
    }

    public static void EnsurePositive(int value, string fieldName)
    {
        if (value <= 0)
            throw new ValidationException($"'{fieldName}' must be greater than zero.");
    }

    public static void EnsureNonNegative(int value, string fieldName)
    {
        if (value < 0)
            throw new ValidationException($"'{fieldName}' cannot be negative.");
    }

    public static void EnsureValidEmail(string? email)
    {
        if (!email.IsValidEmail())
            throw new ValidationException($"'{email}' is not a valid email address.");
    }
}
