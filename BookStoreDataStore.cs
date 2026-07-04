using System.Text.Json;
using BookStoreApp.Domain;
using BookStoreApp.Repositories;

namespace BookStoreApp.Persistence;

/// <summary>
/// Saves and reloads the entire store to/from a single JSON file using
/// async file I/O (requirement 12, bonus). Book's [JsonPolymorphic]
/// attribute is what allows Paperback/Ebook/Audiobook to round-trip
/// correctly through a plain List&lt;Book&gt;.
/// </summary>
public class BookStoreDataStore
{
    private readonly string _filePath;

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
    };

    public BookStoreDataStore(string filePath)
    {
        _filePath = filePath;
    }

    public async Task SaveAsync(IRepository<Book> books, IRepository<Customer> customers, IRepository<Purchase> purchases)
    {
        var data = new PersistedData
        {
            Books = books.GetAll().ToList(),
            Customers = customers.GetAll().ToList(),
            Purchases = purchases.GetAll().ToList(),
        };

        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, data, Options);
    }

    public async Task<PersistedData> LoadAsync()
    {
        if (!File.Exists(_filePath))
            return new PersistedData();

        await using var stream = File.OpenRead(_filePath);
        var data = await JsonSerializer.DeserializeAsync<PersistedData>(stream, Options);
        return data ?? new PersistedData();
    }
}
