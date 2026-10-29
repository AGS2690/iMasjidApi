using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using iMasjid.Application.Common.Models;
using iMasjid.Domain.Exceptions;

namespace iMasjid.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items.TryGetValue(CorrelationIdMiddleware.CorrelationIdItemKey, out var id)
            ? id?.ToString()
            : context.TraceIdentifier;

        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? context.User?.FindFirst("sub")?.Value
                     ?? "Anonymous";

        var statusCode = HttpStatusCode.InternalServerError;
        var message = "An unexpected error occurred while processing your request.";
        List<ErrorDetail>? errors = null;

        switch (exception)
        {
            case FluentValidation.ValidationException fvEx:
                statusCode = HttpStatusCode.BadRequest;
                message = "One or more validation failures occurred.";
                errors = fvEx.Errors
                    .Select(e => new ErrorDetail(e.PropertyName, e.ErrorMessage))
                    .ToList();
                _logger.LogWarning(fvEx, "Validation failed on {Method} {Path} by User: {UserId} (CorrelationId: {CorrelationId})",
                    context.Request.Method, context.Request.Path, userId, correlationId);
                break;

            case ValidationException valEx:
                statusCode = HttpStatusCode.BadRequest;
                message = valEx.Message;
                errors = valEx.Errors
                    .SelectMany(kvp => kvp.Value.Select(msg => new ErrorDetail(kvp.Key, msg)))
                    .ToList();
                _logger.LogWarning(valEx, "Domain validation failure on {Method} {Path} by User: {UserId} (CorrelationId: {CorrelationId})",
                    context.Request.Method, context.Request.Path, userId, correlationId);
                break;

            case NotFoundException notFoundEx:
                statusCode = HttpStatusCode.NotFound;
                message = notFoundEx.Message;
                _logger.LogInformation(notFoundEx, "Resource not found on {Method} {Path} by User: {UserId} (CorrelationId: {CorrelationId})",
                    context.Request.Method, context.Request.Path, userId, correlationId);
                break;

            case UnauthorizedException unauthEx:
                statusCode = HttpStatusCode.Unauthorized;
                message = unauthEx.Message;
                _logger.LogWarning(unauthEx, "Unauthorized access attempt on {Method} {Path} by User: {UserId} (CorrelationId: {CorrelationId})",
                    context.Request.Method, context.Request.Path, userId, correlationId);
                break;

            case ForbiddenException forbidEx:
                statusCode = HttpStatusCode.Forbidden;
                message = forbidEx.Message;
                _logger.LogWarning(forbidEx, "Forbidden action on {Method} {Path} by User: {UserId} (CorrelationId: {CorrelationId})",
                    context.Request.Method, context.Request.Path, userId, correlationId);
                break;

            case ConflictException conflictEx:
                statusCode = HttpStatusCode.Conflict;
                message = conflictEx.Message;
                _logger.LogWarning(conflictEx, "Conflict occurred on {Method} {Path} by User: {UserId} (CorrelationId: {CorrelationId})",
                    context.Request.Method, context.Request.Path, userId, correlationId);
                break;

            case DomainException domainEx:
                statusCode = HttpStatusCode.BadRequest;
                message = domainEx.Message;
                _logger.LogWarning(domainEx, "Domain business rule violation on {Method} {Path} by User: {UserId} (CorrelationId: {CorrelationId})",
                    context.Request.Method, context.Request.Path, userId, correlationId);
                break;

            case DbUpdateException dbEx:
                statusCode = HttpStatusCode.BadRequest;
                message = "A database integrity constraint violation occurred.";
                _logger.LogError(dbEx, "Database update exception on {Method} {Path} by User: {UserId} (CorrelationId: {CorrelationId})",
                    context.Request.Method, context.Request.Path, userId, correlationId);
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError;
                message = _environment.IsDevelopment()
                    ? $"An internal error occurred: {exception.Message}"
                    : "An unexpected error occurred. Please contact support referencing the correlation ID.";
                _logger.LogError(exception, "Unhandled exception occurred on {Method} {Path} by User: {UserId} (CorrelationId: {CorrelationId})",
                    context.Request.Method, context.Request.Path, userId, correlationId);
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse.FailureResult(message, errors, correlationId);

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}
