using Praxis.Backend.Application.Base.Interfaces;
using Praxis.Backend.Application.DB;

namespace Praxis.Backend.Application.Repositories.Interfaces;

public interface INewsRepository : IBaseRepository<News, Guid>
{
    /// <summary>Returns one stable page of active, currently valid News plus the total count.</summary>
    Task<(IReadOnlyList<News> Items, int Total)> ListPublicPageAsync(
        DateTime now, int page, int pageSize, CancellationToken cancellationToken = default);
}
kanns