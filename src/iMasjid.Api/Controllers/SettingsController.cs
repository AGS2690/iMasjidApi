using Microsoft.AspNetCore.Mvc;
using iMasjid.Api.Controllers.Base;
using iMasjid.Api.Filters;
using iMasjid.Application.Common.Models;
using iMasjid.Application.DTOs.Requests.Settings;
using iMasjid.Application.DTOs.Responses.Settings;
using iMasjid.Application.Interfaces.Features;
using iMasjid.Domain.Constants;

namespace iMasjid.Api.Controllers;

public class SettingsController : BaseApiController
{
    private readonly IApplicationSettingService _settingService;

    public SettingsController(IApplicationSettingService settingService)
    {
        _settingService = settingService;
    }

    [HttpGet]
    [HasPermission(Permissions.Settings.View)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SettingResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllSettings(CancellationToken cancellationToken)
    {
        var settings = await _settingService.GetAllAsync(cancellationToken);
        return OkResponse(settings);
    }

    [HttpGet("{key}")]
    [HasPermission(Permissions.Settings.View)]
    [ProducesResponseType(typeof(ApiResponse<SettingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSettingByKey(string key, CancellationToken cancellationToken)
    {
        var setting = await _settingService.GetByKeyAsync(key, cancellationToken);
        return OkResponse(setting);
    }

    [HttpPost]
    [HasPermission(Permissions.Settings.Manage)]
    [ProducesResponseType(typeof(ApiResponse<SettingResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateSetting([FromBody] CreateSettingRequest request, CancellationToken cancellationToken)
    {
        var setting = await _settingService.CreateAsync(request, cancellationToken);
        return CreatedResponse($"/api/settings/{setting.Key}", setting);
    }

    [HttpPut("{key}")]
    [HasPermission(Permissions.Settings.Manage)]
    [ProducesResponseType(typeof(ApiResponse<SettingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSetting(string key, [FromBody] UpdateSettingRequest request, CancellationToken cancellationToken)
    {
        var setting = await _settingService.UpdateAsync(key, request, cancellationToken);
        return OkResponse(setting, "Setting updated successfully.");
    }

    [HttpDelete("{key}")]
    [HasPermission(Permissions.Settings.Manage)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSetting(string key, CancellationToken cancellationToken)
    {
        await _settingService.DeleteAsync(key, cancellationToken);
        return OkResponse("Setting deleted successfully.");
    }
}
