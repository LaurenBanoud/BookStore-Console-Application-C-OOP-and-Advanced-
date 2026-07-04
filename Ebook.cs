using System.Text.Json.Serialization;
using BookStoreApp.Validation;

namespace BookStoreApp.Domain;

public sealed class Ebook : Book
{
    [JsonInclude] public string FileFormat { get; private set; } = "EPUB";
    [JsonInclude] public double FileSizeMb { get; private set; }

    public override string Format => "Ebook";

    [JsonConstructor]
    public Ebook()
    {
    }

    public Ebook(string title, string author, string category, decimal price, int stock, string fileFormat, double fileSizeMb)
        : base(title, author, category, price, stock)
    {
        Validator.EnsureNotEmpty(fileFormat, nameof(fileFormat));
        if (fileSizeMb <= 0)
            throw new ValidationException("'fileSizeMb' must be greater than zero.");

        FileFormat = fileFormat.Trim().ToUpperInvariant();
        FileSizeMb = fileSizeMb;
    }

    public override string ToString() => $"{base.ToString()} | {FileFormat}, {FileSizeMb:0.##} MB";
}
