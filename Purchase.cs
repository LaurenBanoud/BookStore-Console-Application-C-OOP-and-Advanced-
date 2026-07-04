using System.Text.Json.Serialization;
using BookStoreApp.Validation;

namespace BookStoreApp.Domain;

/// <summary>
/// A single sale transaction. Deliberately separate from PurchaseItem
/// (one purchase, many items) so a customer can buy several different
/// books in a single checkout, as required.
/// </summary>
public sealed class Purchase : IEntity
{
    [JsonInclude] public Guid Id { get; private set; }
    [JsonInclude] public Guid CustomerId { get; private set; }
    [JsonInclude] public string CustomerName { get; private set; } = string.Empty;
    [JsonInclude] public DateTime PurchasedAtUtc { get; private set; }
    [JsonInclude] public List<PurchaseItem> Items { get; private set; } = new();

    [JsonIgnore]
    public decimal Total => Items.Sum(i => i.LineTotal);

    [JsonConstructor]
    public Purchase()
    {
    }

    public Purchase(Customer customer, IEnumerable<PurchaseItem> items)
    {
        var itemList = items.ToList();
        if (itemList.Count == 0)
            throw new ValidationException("A purchase must contain at least one book.");

        Id = Guid.NewGuid();
        CustomerId = customer.Id;
        CustomerName = customer.Name;
        PurchasedAtUtc = DateTime.UtcNow;
        Items = itemList;
    }
}
