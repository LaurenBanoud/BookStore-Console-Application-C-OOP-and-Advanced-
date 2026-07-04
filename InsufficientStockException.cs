namespace BookStoreApp.Domain;

/// <summary>
/// Thrown when a sale would take a book's stock below zero. Kept as its own
/// type (instead of a generic exception) so callers can catch it specifically
/// and give the user a friendly, targeted message.
/// </summary>
public class InsufficientStockException : Exception
{
    public InsufficientStockException(string message) : base(message)
    {
    }
}
