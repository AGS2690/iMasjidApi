using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using iMasjid.Application.Common.Models;
using iMasjid.Application.Interfaces.Persistence;
using iMasjid.Application.Interfaces.Services;
using iMasjid.Domain.Entities;
using iMasjid.Domain.Enums;
using iMasjid.Infrastructure.Persistence;

namespace iMasjid.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly AppDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IServiceScopeFactory _scopeFactory;

    public AuditService(
        AppDbContext context,
        IDateTimeProvider dateTimeProvider,
        IServiceScopeFactory scopeFactory)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
        _scopeFactory = scopeFactory;
    }

    public async Task LogAsync(
        string? userId,
        AuditAction action,
        string entityName,
        string? entityId,
        object? oldValues,
        object? newValues,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        var audit = new ImAuditLog
        {
            UserId = userId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedOn = _dateTimeProvider.UtcNow
        };

        using var scope = _scopeFactory.CreateScope();
        var auditContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await auditContext.ImAuditLogs.AddAsync(audit, cancellationToken);
        await auditContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResponse<AuditLogRecord>> GetAuditLogsPagedAsync(
        PagedRequest request,
        string? entityName = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ImAuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityName))
        {
            query = query.Where(a => a.EntityName == entityName);
        }

        if (!string.IsNullOrWhiteSpace(userId))
        {
            query = query.Where(a => a.UserId == userId);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.CreatedOn)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AuditLogRecord(
                a.Id,
                a.UserId,
                a.Action,
                a.EntityName,
                a.EntityId,
                a.OldValues,
                a.NewValues,
                a.IpAddress,
                a.UserAgent,
                a.CreatedOn))
            .ToListAsync(cancellationToken);

        return PagedResponse<AuditLogRecord>.Create(items, totalCount, request.PageNumber, request.PageSize);
    }
}
