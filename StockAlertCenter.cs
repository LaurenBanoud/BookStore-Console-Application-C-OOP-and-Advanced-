using BookStoreApp.Domain;

namespace BookStoreApp.Services;

/// <summary>
/// A single, static notification hub that any part of the system can
/// subscribe to (the console UI, a future email service, a logger, ...)
/// to react whenever any book runs out of stock (requirement 10). Books
/// raise their own OutOfStock event; this class re-broadcasts it so
/// subscribers don't need a reference to every individual Book instance.
/// </summary>
public static class StockAlertCenter
{
    public static event EventHandler<BookStockEventArgs>? BookOutOfStock;

    /// <summary>Wires a book's own OutOfStock event into the central hub.</summary>
    public static void Register(Book book)
    {
        book.OutOfStock += (sender, args) => BookOutOfStock?.Invoke(sender, args);
    }
}
