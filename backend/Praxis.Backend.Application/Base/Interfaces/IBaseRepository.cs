namespace Praxis.Backend.Application.Base.Interfaces;

/// <summary>Common CRUD surface shared by every entity repository.</summary>
public interface IBaseRepository<TEntity, in TKey>
    where TEntity : class, IEntityWithId<TKey>
{
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>Inserts the entity, or overwrites an existing row with the same id.</summary>
    Task UpsertAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task UpsertManyAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    Task<int> CountByIdsAsync(IEnumerable<TKey> ids, CancellationToken cancellationToken = default);

    Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Round-trips a trivial query against the database to verify connectivity.</summary>
    Task PingAsync(CancellationToken cancellationToken = default);
}
