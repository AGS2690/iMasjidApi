using AutoMapper;
using iMasjid.Application.Common.Models;
using iMasjid.Application.DTOs.Responses.AuditLogs;
using iMasjid.Application.Interfaces.Features;
using iMasjid.Application.Interfaces.Persistence;
using iMasjid.Domain.Entities;

namespace iMasjid.Application.Features.AuditLogs;

public class AuditLogService : IAuditLogService
{
    private readonly IGenericRepository<ImAuditLog> _auditLogRepository;
    private readonly IMapper _mapper;

    public AuditLogService(IGenericRepository<ImAuditLog> auditLogRepository, IMapper mapper)
    {
        _auditLogRepository = auditLogRepository;
        _mapper = mapper;
    }

    public async Task<PagedResponse<AuditLogResponse>> GetPagedLogsAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var pagedLogs = await _auditLogRepository.GetPagedListAsync(
            request,
            orderBy: q => q.OrderByDescending(a => a.CreatedOn),
            cancellationToken: cancellationToken);

        var responses = _mapper.Map<IReadOnlyList<AuditLogResponse>>(pagedLogs.Items);

        return PagedResponse<AuditLogResponse>.Create(
            responses,
            pagedLogs.TotalCount,
            pagedLogs.PageNumber,
            pagedLogs.PageSize);
    }
}
