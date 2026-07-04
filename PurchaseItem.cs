using System.Text.Json.Serialization;
using BookStoreApp.Validation;

namespace BookStoreApp.Domain;

/// <summary>
/// One line of a purchase. Snapshots the price at the time of sale so that a
/// later price change never rewrites the history of what a customer actually paid.
/// </summary>
public sealed class PurchaseItem
{
    [JsonInclude] public Guid BookId { get; private set; }
    [JsonInclude] public string BookTitle { get; private set; } = string.Empty;
    [JsonInclude] public int Quantity { get; private set; }
    [JsonInclude] public decimal UnitPriceAtSale { get; private set; }

    [JsonIgnore]
    public decimal LineTotal => Quantity * UnitPriceAtSale;

    [JsonConstructor]
    public PurchaseItem()
    {
    }

    public PurchaseItem(Book book, int quantity)
    {
        Validator.EnsurePositive(quantity, nameof(quantity));

        BookId = book.Id;
        BookTitle = book.Title;
        Quantity = quantity;
        UnitPriceAtSale = book.Price;
    }
}
