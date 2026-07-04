using BookStoreApp.Domain;
using BookStoreApp.Extensions;
using BookStoreApp.Repositories;
using BookStoreApp.Validation;

namespace BookStoreApp.Services;

public class BookCatalogService
{
    private readonly IRepository<Book> _books;

    public BookCatalogService(IRepository<Book> books)
    {
        _books = books;
    }

    public Book AddBook(Book book)
    {
        _books.Add(book);
        StockAlertCenter.Register(book);
        return book;
    }

    public bool RemoveBook(Guid id) => _books.Remove(id);

    public Book? GetById(Guid id) => _books.GetById(id);

    public IReadOnlyList<Book> GetAll() => _books.GetAll();

    public IEnumerable<Book> Search(string term) =>
        _books.Find(b =>
            b.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            b.Author.Contains(term, StringComparison.OrdinalIgnoreCase));

    public IEnumerable<Book> FilterByCategory(string category) => _books.GetAll().ByCategory(category);

    public IEnumerable<Book> FilterByAuthor(string author) => _books.GetAll().ByAuthor(author);

    public IEnumerable<Book> FilterByPriceRange(decimal min, decimal max)
    {
        if (min < 0 || max < 0)
            throw new ValidationException("Prices cannot be negative.");
        if (min > max)
            throw new ValidationException("Minimum price cannot be greater than maximum price.");

        return _books.GetAll().InPriceRange(min, max);
    }

    public void ApplyRuleToBooks(IEnumerable<Book> books, Action<Book> rule) => books.ApplyRule(rule);

    /// <summary>
    /// Re-wires stock alert subscriptions after bulk-loading books from disk,
    /// since events are never persisted.
    /// </summary>
    public void RegisterAllForAlerts()
    {
        foreach (var book in _books.GetAll())
            StockAlertCenter.Register(book);
    }
}
