using System.Text.Json.Serialization;
using BookStoreApp.Validation;

namespace BookStoreApp.Domain;

public sealed class Paperback : Book
{
    [JsonInclude] public int PageCount { get; private set; }

    public override string Format => "Paperback";

    [JsonConstructor]
    public Paperback()
    {
    }

    public Paperback(string title, string author, string category, decimal price, int stock, int pageCount)
        : base(title, author, category, price, stock)
    {
        Validator.EnsurePositive(pageCount, nameof(pageCount));
        PageCount = pageCount;
    }

    public override string ToString() => $"{base.ToString()} | {PageCount} pages";
}
