using System.Text.Json.Serialization;
using BookStoreApp.Validation;

namespace BookStoreApp.Domain;

public sealed class Customer : IEntity
{
    [JsonInclude] public Guid Id { get; private set; }
    [JsonInclude] public string Name { get; private set; } = string.Empty;
    [JsonInclude] public string Email { get; private set; } = string.Empty;

    [JsonConstructor]
    public Customer()
    {
    }

    public Customer(string name, string email)
    {
        Validator.EnsureNotEmpty(name, nameof(name));
        Validator.EnsureValidEmail(email);

        Id = Guid.NewGuid();
        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
    }

    public override string ToString() => $"{Name} <{Email}>";
}
