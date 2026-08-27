using Praxis.Backend.Application.DB;
using Praxis.Backend.Application.Repositories.Interfaces;

namespace Praxis.Backend.Tests.Api;

public sealed class FakeNewsRepository(IReadOnlyList<News> items, int total) : INewsRepository
{
    public bool IsHealthy { get; init; } = true;

    public Task<News?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task UpsertAsync(News entity, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task UpsertManyAsync(IEnumerable<News> entities, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<int> CountByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<(IReadOnlyList<News> Items, int Total)> ListPublicPageAsync(
        DateTime now, int page, int pageSize, CancellationToken cancellationToken = default) =>
        Task.FromResult((items, total));

    public Task PingAsync(CancellationToken cancellationToken = default) =>
        IsHealthy ? Task.CompletedTask : Task.FromException(new FakeDbException("connection refused"));
}

public sealed class FailingNewsRepository : INewsRepository
{
    public Task<News?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task UpsertAsync(News entity, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task UpsertManyAsync(IEnumerable<News> entities, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<int> CountByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<(IReadOnlyList<News> Items, int Total)> ListPublicPageAsync(
        DateTime now, int page, int pageSize, CancellationToken cancellationToken = default) =>
        throw new FakeDbException("connection refused");

    public Task PingAsync(CancellationToken cancellationToken = default) =>
        throw new FakeDbException("connection refused");
}
