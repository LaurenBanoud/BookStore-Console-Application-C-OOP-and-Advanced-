using BookStoreApp.Domain;

namespace BookStoreApp.Persistence;

/// <summary>The single JSON document that holds the whole store's state.</summary>
public class PersistedData
{
    public List<Book> Books { get; set; } = new();
    public List<Customer> Customers { get; set; } = new();
    public List<Purchase> Purchases { get; set; } = new();
}
