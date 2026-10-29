using iMasjid.Application.Common.Models;
using iMasjid.Domain.Enums;

namespace iMasjid.Application.Interfaces.Services;

public interface IAuditService
{
    Task LogAsync(string? userId, AuditAction action, string entityName, string? entityId, object? oldValues, object? newValues, string? ipAddress = null, string? userAgent = null, CancellationToken cancellationToken = default);
    Task<PagedResponse<AuditLogRecord>> GetAuditLogsPagedAsync(PagedRequest request, string? entityName = null, string? userId = null, CancellationToken cancellationToken = default);
}

public record AuditLogRecord(
    Guid Id,
    string? UserId,
    AuditAction Action,
    string EntityName,
    string? EntityId,
    string? OldValues,
    string? NewValues,
    string? IpAddress,
    string? UserAgent,
    DateTime CreatedOn);
