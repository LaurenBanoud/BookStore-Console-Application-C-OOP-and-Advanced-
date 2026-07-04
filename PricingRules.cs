using BookStoreApp.Domain;
using BookStoreApp.Extensions;

namespace BookStoreApp.Rules;

/// <summary>
/// A small library of ready-made rules that can be handed to
/// BookQueryExtensions.ApplyRule. Adding a new rule is just adding a new
/// static method here - it never requires touching the books, the
/// repository, or the menu code that applies rules.
/// </summary>
public static class PricingRules
{
    public static Action<Book> PercentageDiscount(decimal percent) =>
        book => book.ChangePrice(book.Price.ApplyPercentageDiscount(percent));

    public static Action<Book> FlatPriceIncrease(decimal amount) =>
        book => book.ChangePrice(book.Price + amount);

    public static Action<Book> SetMinimumPrice(decimal minimumPrice) =>
        book =>
        {
            if (book.Price < minimumPrice)
                book.ChangePrice(minimumPrice);
        };
}
