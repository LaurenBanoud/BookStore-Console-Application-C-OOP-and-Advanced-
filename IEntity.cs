namespace BookStoreApp.Domain;

/// <summary>
/// Anything that can be stored in the generic repository must expose a unique Id.
/// This is the only contract the repository depends on, which is what lets the
/// same repository implementation store books, customers, purchases, or any
/// future entity without changes.
/// </summary>
public interface IEntity
{
    Guid Id { get; }
}
