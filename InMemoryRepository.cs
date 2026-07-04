using System.Collections.Concurrent;
using BookStoreApp.Domain;

namespace BookStoreApp.Repositories;

/// <summary>
/// Thread-safe, generic in-memory store. A ConcurrentDictionary is used
/// instead of a plain Dictionary/List so multiple purchases can read and
/// write the collection at the same time without corrupting it
/// (requirement 13) or needing a caller-managed lock.
/// </summary>
public class InMemoryRepository<T> : IRepository<T> where T : IEntity
{
    private readonly ConcurrentDictionary<Guid, T> _store = new();

    public void Add(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (!_store.TryAdd(entity.Id, entity))
            throw new InvalidOperationException($"An entity with id '{entity.Id}' already exists.");
    }

    public bool Remove(Guid id) => _store.TryRemove(id, out _);

    public T? GetById(Guid id) => _store.TryGetValue(id, out var value) ? value : default;

    public IReadOnlyList<T> GetAll() => _store.Values.ToList();

    public IEnumerable<T> Find(Func<T, bool> predicate) => _store.Values.Where(predicate).ToList();

    public void ReplaceAll(IEnumerable<T> entities)
    {
        _store.Clear();
        foreach (var entity in entities)
            _store[entity.Id] = entity;
    }
}
