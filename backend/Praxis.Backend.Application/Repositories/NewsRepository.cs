using Microsoft.EntityFrameworkCore;
using Praxis.Backend.Application.Base;
using Praxis.Backend.Application.DB;
using Praxis.Backend.Application.Repositories.Interfaces;

namespace Praxis.Backend.Application.Repositories;

public sealed class NewsRepository(NewsDbContext context) : BaseRepository<News, Guid>(context), INewsRepository
{
    public async Task<(IReadOnlyList<News> Items, int Total)> ListPublicPageAsync(
        DateTime now, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = Set.Where(n =>
            n.IsActive
            && n.PublishedAt <= now
            && n.ValidFrom <= now
            && (n.ValidUntil == null || n.ValidUntil > now));

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(n => n.PublishedAt)
            .ThenByDescending(n => n.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }
}
