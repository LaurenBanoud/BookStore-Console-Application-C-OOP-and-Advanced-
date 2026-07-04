using BookStoreApp.Domain;

namespace BookStoreApp.Repositories;

/// <summary>
/// A storage contract that works for any entity in the system - books,
/// customers, purchases, or anything added later - as long as it implements
/// IEntity. This is what makes the repository generic and reusable
/// (requirement 8) rather than one class per entity type.
/// </summary>
public interface IRepository<T> where T : IEntity
{
    void Add(T entity);
    bool Remove(Guid id);
    T? GetById(Guid id);
    IReadOnlyList<T> GetAll();
    IEnumerable<T> Find(Func<T, bool> predicate);

    /// <summary>Used by persistence loading to repopulate the store in one shot.</summary>
    void ReplaceAll(IEnumerable<T> entities);
}
