using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using iMasjid.Api.Middleware;
using iMasjid.Application.Common.Models;

namespace iMasjid.Api.Controllers.Base;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[EnableRateLimiting("DynamicEndpointRateLimitPolicy")]
public abstract class BaseApiController : ControllerBase
{
    protected string? CorrelationId => HttpContext.Items.TryGetValue(CorrelationIdMiddleware.CorrelationIdItemKey, out var id)
        ? id?.ToString()
        : HttpContext.TraceIdentifier;

    protected Guid? CurrentUserId
    {
        get
        {
            var claimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                             ?? User.FindFirst("sub")?.Value;

            return Guid.TryParse(claimValue, out var userId) ? userId : null;
        }
    }

    protected string? CurrentUserName => User.Identity?.Name
        ?? User.FindFirst(ClaimTypes.Name)?.Value
        ?? User.FindFirst("name")?.Value;

    protected string? GetIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    protected IActionResult OkResponse<T>(T data, string message = "Operation completed successfully.")
    {
        var response = ApiResponse<T>.SuccessResult(data, message);
        response.CorrelationId = CorrelationId;
        return Ok(response);
    }

    protected IActionResult OkResponse(string message = "Operation completed successfully.")
    {
        var response = ApiResponse.SuccessResult(message);
        response.CorrelationId = CorrelationId;
        return Ok(response);
    }

    protected IActionResult CreatedResponse<T>(string uri, T data, string message = "Resource created successfully.")
    {
        var response = ApiResponse<T>.SuccessResult(data, message);
        response.CorrelationId = CorrelationId;
        return Created(uri, response);
    }

    protected IActionResult PagedOkResponse<T>(PagedResponse<T> pagedResponse, string message = "Operation completed successfully.")
    {
        var response = ApiResponse<PagedResponse<T>>.SuccessResult(pagedResponse, message);
        response.CorrelationId = CorrelationId;
        return Ok(response);
    }
}
