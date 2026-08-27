using System.Data.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Praxis.Backend.Application.Base;
using Praxis.Backend.Application.Repositories.Interfaces;

namespace Praxis.Backend.Application.Controller;

[Route("health")]
public sealed class HealthController(INewsRepository repository) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            await repository.PingAsync();
            return Ok(new { status = "ok" });
        }
        catch (DbException)
        {
            return ProblemResponse(
                StatusCodes.Status503ServiceUnavailable,
                "news_service_unavailable",
                "The News Service is temporarily unavailable.");
        }
    }
}
