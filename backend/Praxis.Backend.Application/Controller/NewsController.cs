using System.Data.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Praxis.Backend.Application.Base;
using Praxis.Backend.Application.Dto;
using Praxis.Backend.Application.Repositories.Interfaces;

namespace Praxis.Backend.Application.Controller;

[Route("api/news")]
public sealed class NewsController(INewsRepository repository, ILogger<NewsController> logger) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return ProblemResponse(
                StatusCodes.Status422UnprocessableEntity,
                "invalid_request",
                "One or more query parameters are invalid.");
        }

        try
        {
            var now = DateTime.UtcNow;
            var (items, total) = await repository.ListPublicPageAsync(now, page, pageSize);
            var body = new NewsPageDto(
                items.Select(n => new NewsItemDto(
                    n.Id, n.Title, n.Summary, n.Content, n.PublishedAt, n.ValidFrom, n.ValidUntil)).ToList(),
                page,
                pageSize,
                total,
                page * pageSize < total);
            return Ok(body);
        }
        catch (DbException ex)
        {
            logger.LogError(ex, "News Service database access failed for /api/news");
            return ProblemResponse(
                StatusCodes.Status503ServiceUnavailable,
                "news_service_unavailable",
                "The News Service is temporarily unavailable.");
        }
    }
}
