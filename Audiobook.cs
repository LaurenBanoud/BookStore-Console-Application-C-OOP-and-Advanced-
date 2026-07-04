using System.Text.Json.Serialization;
using BookStoreApp.Validation;

namespace BookStoreApp.Domain;

public sealed class Audiobook : Book
{
    [JsonInclude] public int DurationMinutes { get; private set; }
    [JsonInclude] public string Narrator { get; private set; } = string.Empty;

    public override string Format => "Audiobook";

    [JsonConstructor]
    public Audiobook()
    {
    }

    public Audiobook(string title, string author, string category, decimal price, int stock, int durationMinutes, string narrator)
        : base(title, author, category, price, stock)
    {
        Validator.EnsurePositive(durationMinutes, nameof(durationMinutes));
        Validator.EnsureNotEmpty(narrator, nameof(narrator));

        DurationMinutes = durationMinutes;
        Narrator = narrator.Trim();
    }

    public override string ToString() => $"{base.ToString()} | {DurationMinutes} min, narrated by {Narrator}";
}
