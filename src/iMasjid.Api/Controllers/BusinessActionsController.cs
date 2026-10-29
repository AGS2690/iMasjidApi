using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using iMasjid.Api.Controllers.Base;
using iMasjid.Api.Filters;
using iMasjid.Application.Common.Models;
using iMasjid.Application.DTOs.Requests.Actions;
using iMasjid.Application.DTOs.Responses.Actions;
using iMasjid.Application.Interfaces.Features;
using iMasjid.Domain.Constants;

namespace iMasjid.Api.Controllers;

[Authorize]
[Route("api/actions")]
[Route("api/businessactions")]
public class BusinessActionsController : BaseApiController
{
    private readonly IBusinessActionService _actionService;

    public BusinessActionsController(IBusinessActionService actionService)
    {
        _actionService = actionService;
    }

    [HttpGet]
    [HasPermission(Permissions.Actions.View)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<BusinessActionResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive, CancellationToken cancellationToken)
    {
        var actions = await _actionService.GetAllAsync(isActive, cancellationToken);
        return OkResponse(actions, "Business actions retrieved successfully.");
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Actions.View)]
    [ProducesResponseType(typeof(ApiResponse<BusinessActionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var action = await _actionService.GetByIdAsync(id, cancellationToken);
        return OkResponse(action, "Business action retrieved successfully.");
    }

    [HttpPost]
    [HasPermission(Permissions.Actions.Create)]
    [ProducesResponseType(typeof(ApiResponse<BusinessActionResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBusinessActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _actionService.CreateAsync(request, cancellationToken);
        return CreatedResponse($"/api/actions/{result.Id}", result, "Business action created successfully.");
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Actions.Edit)]
    [ProducesResponseType(typeof(ApiResponse<BusinessActionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBusinessActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _actionService.UpdateAsync(id, request, cancellationToken);
        return OkResponse(result, "Business action updated successfully.");
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Actions.Delete)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _actionService.DeleteAsync(id, cancellationToken);
        return OkResponse("Business action deleted successfully.");
    }
}
