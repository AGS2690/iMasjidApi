using Microsoft.AspNetCore.Mvc;
using iMasjid.Api.Controllers.Base;
using iMasjid.Api.Filters;
using iMasjid.Application.Common.Models;
using iMasjid.Application.DTOs.Responses.AuditLogs;
using iMasjid.Application.Interfaces.Features;
using iMasjid.Domain.Constants;

namespace iMasjid.Api.Controllers;

public class AuditLogsController : BaseApiController
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    [HasPermission(Permissions.AuditLogs.View)]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<AuditLogResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLogs([FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        var logs = await _auditLogService.GetPagedLogsAsync(request, cancellationToken);
        return PagedOkResponse(logs);
    }
}
