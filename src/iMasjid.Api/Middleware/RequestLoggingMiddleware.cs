using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace iMasjid.Api.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var statusCode = context.Response.StatusCode;
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            if (statusCode >= 500)
            {
                _logger.LogError("HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
                    context.Request.Method, context.Request.Path, statusCode, elapsedMs);
            }
            else if (statusCode >= 400)
            {
                _logger.LogWarning("HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
                    context.Request.Method, context.Request.Path, statusCode, elapsedMs);
            }
            else
            {
                _logger.LogInformation("HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
                    context.Request.Method, context.Request.Path, statusCode, elapsedMs);
            }
        }
    }
}
