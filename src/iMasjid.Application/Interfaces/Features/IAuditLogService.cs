using iMasjid.Application.Common.Models;
using iMasjid.Application.DTOs.Responses.AuditLogs;

namespace iMasjid.Application.Interfaces.Features;

public interface IAuditLogService
{
    Task<PagedResponse<AuditLogResponse>> GetPagedLogsAsync(PagedRequest request, CancellationToken cancellationToken = default);
}
