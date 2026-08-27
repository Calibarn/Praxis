using Praxis.Backend.Application.Base.Interfaces;

namespace Praxis.Backend.Application.DB;

/// <summary>News persistence model and source of truth for the News domain.</summary>
public sealed class News : IEntityWithId<Guid>
{
    public required Guid Id { get; init; }
    public required string Title { get; set; }
    public required string Summary { get; set; }
    public required string Content { get; set; }
    public required DateTime PublishedAt { get; set; }
    public required DateTime ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
