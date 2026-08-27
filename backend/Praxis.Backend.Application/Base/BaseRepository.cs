using Microsoft.EntityFrameworkCore;
using Praxis.Backend.Application.Base.Interfaces;

namespace Praxis.Backend.Application.Base;

/// <summary>
/// EF Core CRUD implementation shared by every entity repository, so concrete
/// repositories only need to add their own query methods.
/// </summary>
public abstract class BaseRepository<TEntity, TKey>(DbContext context) : IBaseRepository<TEntity, TKey>
    where TEntity : class, IEntityWithId<TKey>
{
    protected DbContext Context { get; } = context;

    protected DbSet<TEntity> Set { get; } = context.Set<TEntity>();

    public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default) =>
        await Set.FindAsync([id], cancellationToken);

    public virtual async Task UpsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(entity.Id, cancellationToken);
        if (existing is null)
        {
            Set.Add(entity);
            return;
        }

        Context.Entry(existing).CurrentValues.SetValues(entity);
    }

    public virtual async Task UpsertManyAsync(
        IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
        {
            await UpsertAsync(entity, cancellationToken);
        }
    }

    public virtual Task<int> CountByIdsAsync(
        IEnumerable<TKey> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToArray();
        return Set.CountAsync(e => idList.Contains(e.Id), cancellationToken);
    }

    public virtual async Task DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(id, cancellationToken);
        if (existing is not null)
        {
            Set.Remove(existing);
        }
    }

    public virtual Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        Context.SaveChangesAsync(cancellationToken);

    public virtual async Task PingAsync(CancellationToken cancellationToken = default)
    {
        await Context.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);
    }
}
