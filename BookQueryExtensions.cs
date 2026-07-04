using BookStoreApp.Domain;

namespace BookStoreApp.Extensions;

/// <summary>
/// LINQ-style extension methods for filtering books and for applying any
/// developer-chosen rule to a list of books (requirement 9 - e.g. a discount
/// or a price adjustment, expressed simply as an Action&lt;Book&gt;).
/// </summary>
public static class BookQueryExtensions
{
    /// <summary>
    /// Applies any rule a developer chooses to every book in the sequence.
    /// Because the rule is just a delegate, callers can pass a built-in
    /// pricing rule (see Rules.PricingRules) or an ad-hoc lambda.
    /// </summary>
    public static void ApplyRule(this IEnumerable<Book> books, Action<Book> rule)
    {
        foreach (var book in books)
            rule(book);
    }

    public static IEnumerable<Book> ByCategory(this IEnumerable<Book> books, string category) =>
        books.Where(b => b.Category.Equals(category, StringComparison.OrdinalIgnoreCase));

    public static IEnumerable<Book> ByAuthor(this IEnumerable<Book> books, string author) =>
        books.Where(b => b.Author.Contains(author, StringComparison.OrdinalIgnoreCase));

    public static IEnumerable<Book> InPriceRange(this IEnumerable<Book> books, decimal min, decimal max) =>
        books.Where(b => b.Price.IsBetween(min, max));

    public static IEnumerable<Book> InStockOnly(this IEnumerable<Book> books) =>
        books.Where(b => b.Stock > 0);
}
