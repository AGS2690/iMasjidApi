using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace iMasjid.Api.Middleware;

public class CorrelationIdMiddleware
{
    public const string CorrelationIdHeaderName = "X-Correlation-ID";
    public const string CorrelationIdItemKey = "CorrelationId";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var headerVal) && !string.IsNullOrWhiteSpace(headerVal)
            ? headerVal.ToString()
            : Guid.NewGuid().ToString("N");

        context.Items[CorrelationIdItemKey] = correlationId;
        context.Response.Headers[CorrelationIdHeaderName] = correlationId;

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            [CorrelationIdItemKey] = correlationId
        }))
        {
            await _next(context);
        }
    }
}
