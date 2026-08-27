namespace Praxis.Backend.Application.Dto;

/// <summary>Public representation of one News record. Fields are always plain text.</summary>
public sealed record NewsItemDto(
    Guid Id,
    string Title,
    string Summary,
    string Content,
    DateTime PublishedAt,
    DateTime ValidFrom,
    DateTime? ValidUntil);

/// <summary>One stable, sorted page of the public News listing.</summary>
public sealed record NewsPageDto(
    IReadOnlyList<NewsItemDto> Items,
    int Page,
    int PageSize,
    int Total,
    bool HasMore);

/// <summary>Stable error envelope that never leaks internal details.</summary>
public sealed record ProblemDto(string Code, string Message);
