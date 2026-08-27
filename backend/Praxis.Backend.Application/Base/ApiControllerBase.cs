using Microsoft.AspNetCore.Mvc;
using Praxis.Backend.Application.Dto;

namespace Praxis.Backend.Application.Base;

/// <summary>Shared base for API controllers: a stable Problem envelope for error responses.</summary>
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult ProblemResponse(int statusCode, string code, string message) =>
        StatusCode(statusCode, new ProblemDto(code, message));
}
