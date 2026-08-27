using Praxis.Backend.Application.DB;

namespace Praxis.Backend.Application.Service.Seed;

public sealed record NewsSeed(
    Guid Id,
    string Environment,
    string Title,
    string Summary,
    string Content,
    DateTime PublishedAt,
    DateTime ValidFrom,
    DateTime? ValidUntil = null,
    bool IsActive = true)
{
    public News ToModel() => new()
    {
        Id = Id,
        Title = Title,
        Summary = Summary,
        Content = Content,
        PublishedAt = PublishedAt,
        ValidFrom = ValidFrom,
        ValidUntil = ValidUntil,
        IsActive = IsActive,
    };
}
