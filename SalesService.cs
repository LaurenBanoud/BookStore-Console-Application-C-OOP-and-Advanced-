using BookStoreApp.Domain;
using BookStoreApp.Repositories;
using BookStoreApp.Validation;

namespace BookStoreApp.Services;

public class SalesService
{
    private readonly IRepository<Purchase> _purchases;
    private readonly IRepository<Book> _books;

    public SalesService(IRepository<Purchase> purchases, IRepository<Book> books)
    {
        _purchases = purchases;
        _books = books;
    }

    /// <summary>
    /// Records a purchase of one or more books for a customer. Safe to call
    /// concurrently (requirement 13): each Book instance guards its own
    /// stock with an internal lock, so two purchases for two different
    /// books proceed in parallel, while two purchases racing for the same
    /// book are serialized just long enough to keep the count correct.
    /// If any line item can't be fulfilled, everything already decremented
    /// for this purchase is rolled back so stock stays consistent.
    /// </summary>
    public Purchase RecordPurchase(Customer customer, IReadOnlyDictionary<Guid, int> bookQuantities)
    {
        if (bookQuantities is null || bookQuantities.Count == 0)
            throw new ValidationException("A purchase must contain at least one book.");

        var resolved = new List<(Book Book, int Quantity)>();
        foreach (var (bookId, quantity) in bookQuantities)
        {
            var book = _books.GetById(bookId)
                       ?? throw new ValidationException($"Book with id '{bookId}' was not found.");
            Validator.EnsurePositive(quantity, "quantity");
            resolved.Add((book, quantity));
        }

        var decremented = new List<(Book Book, int Quantity)>();
        try
        {
            foreach (var (book, quantity) in resolved)
            {
                book.DecreaseStock(quantity);
                decremented.Add((book, quantity));
            }
        }
        catch
        {
            foreach (var (book, quantity) in decremented)
                book.IncreaseStock(quantity);
            throw;
        }

        var items = resolved.Select(r => new PurchaseItem(r.Book, r.Quantity));
        var purchase = new Purchase(customer, items);
        _purchases.Add(purchase);
        return purchase;
    }

    public IReadOnlyList<Purchase> GetAll() => _purchases.GetAll();

    public decimal TotalRevenue() => _purchases.GetAll().Sum(p => p.Total);

    public (string Title, int CopiesSold)? BestSellingBook()
    {
        var best = _purchases.GetAll()
            .SelectMany(p => p.Items)
            .GroupBy(i => i.BookTitle)
            .Select(g => (Title: g.Key, CopiesSold: g.Sum(i => i.Quantity)))
            .OrderByDescending(x => x.CopiesSold)
            .FirstOrDefault();

        return best == default ? null : best;
    }

    public (string Name, decimal TotalSpent)? TopCustomer()
    {
        var top = _purchases.GetAll()
            .GroupBy(p => p.CustomerName)
            .Select(g => (Name: g.Key, TotalSpent: g.Sum(p => p.Total)))
            .OrderByDescending(x => x.TotalSpent)
            .FirstOrDefault();

        return top == default ? null : top;
    }
}
