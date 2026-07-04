using System.Text.Json.Serialization;
using BookStoreApp.Extensions;
using BookStoreApp.Validation;

namespace BookStoreApp.Domain;

/// <summary>
/// Base type for every kind of book the store can sell. Format-specific
/// behaviour (page count, file size, narrator, ...) lives entirely in the
/// subclasses. Nothing outside this file needs to know a new format exists
/// as long as it works through the <see cref="Book"/> reference - that is
/// what satisfies requirement 7 (new formats without touching existing code)
/// and is a direct application of the Open/Closed Principle.
///
/// [JsonPolymorphic]/[JsonDerivedType] tell System.Text.Json how to save and
/// reload the concrete subtype (requirement 12).
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "format")]
[JsonDerivedType(typeof(Paperback), "paperback")]
[JsonDerivedType(typeof(Ebook), "ebook")]
[JsonDerivedType(typeof(Audiobook), "audiobook")]
public abstract class Book : IEntity
{
    [JsonInclude] public Guid Id { get; private set; }
    [JsonInclude] public string Title { get; private set; } = string.Empty;
    [JsonInclude] public string Author { get; private set; } = string.Empty;
    [JsonInclude] public string Category { get; private set; } = string.Empty;
    [JsonInclude] public decimal Price { get; private set; }
    [JsonInclude] public int Stock { get; private set; }

    /// <summary>Human-readable format name, implemented by each subclass.</summary>
    [JsonIgnore]
    public abstract string Format { get; }

    // Guards Stock so concurrent purchases can never push it below zero
    // (requirement 13). Each Book instance owns its own lock, so selling
    // two different books at the same time never blocks on each other.
    private readonly object _stockLock = new();

    /// <summary>
    /// Raised the instant this book's stock reaches zero. Anything in the
    /// system can subscribe to react to it (requirement 10) - see
    /// Services.StockAlertCenter for the central hookup point.
    /// </summary>
    public event EventHandler<BookStockEventArgs>? OutOfStock;

    /// <summary>Reserved for JSON deserialization only.</summary>
    protected Book()
    {
    }

    protected Book(string title, string author, string category, decimal price, int stock)
    {
        Validator.EnsureNotEmpty(title, nameof(title));
        Validator.EnsureNotEmpty(author, nameof(author));
        Validator.EnsureNotEmpty(category, nameof(category));
        Validator.EnsurePositive(price, nameof(price));
        Validator.EnsureNonNegative(stock, nameof(stock));

        Id = Guid.NewGuid();
        Title = title.Trim();
        Author = author.Trim();
        Category = category.Trim();
        Price = price;
        Stock = stock;
    }

    public void DecreaseStock(int quantity)
    {
        Validator.EnsurePositive(quantity, nameof(quantity));

        lock (_stockLock)
        {
            if (Stock < quantity)
            {
                throw new InsufficientStockException(
                    $"Cannot sell {quantity} cop{(quantity == 1 ? "y" : "ies")} of '{Title}'. " +
                    $"Only {Stock} left in stock.");
            }

            Stock -= quantity;

            if (Stock == 0)
                OutOfStock?.Invoke(this, new BookStockEventArgs(this));
        }
    }

    public void IncreaseStock(int quantity)
    {
        Validator.EnsurePositive(quantity, nameof(quantity));
        lock (_stockLock)
        {
            Stock += quantity;
        }
    }

    public void ChangePrice(decimal newPrice)
    {
        Validator.EnsurePositive(newPrice, nameof(newPrice));
        Price = newPrice;
    }

    public override string ToString() =>
        $"{Title} — {Author} [{Category}] ({Format}) | {Price.ToCurrency()} | Stock: {Stock}";
}
