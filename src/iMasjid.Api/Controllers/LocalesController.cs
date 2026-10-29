using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using iMasjid.Api.Controllers.Base;
using iMasjid.Application.Common.Models;
using iMasjid.Application.DTOs.Responses.Localization;
using iMasjid.Application.Interfaces.Features;

namespace iMasjid.Api.Controllers;

public class LocalesController : BaseApiController
{
    private readonly ILocaleService _localeService;

    public LocalesController(ILocaleService localeService)
    {
        _localeService = localeService;
    }

    /// <summary>
    /// Gets the list of supported application languages and locales.
    /// </summary>
    /// <param name="onlyActive">Optional filter to return only active languages (defaults to true).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("supported-languages")]
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SupportedLanguageResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSupportedLanguages([FromQuery] bool onlyActive = true, CancellationToken cancellationToken = default)
    {
        var languages = await _localeService.GetSupportedLanguagesAsync(onlyActive, cancellationToken);
        return OkResponse(languages, "Supported languages retrieved successfully.");
    }

    /// <summary>
    /// Gets a supported locale by its unique identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<SupportedLanguageResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var locale = await _localeService.GetByIdAsync(id, cancellationToken);
        if (locale == null)
            return NotFound(ApiResponse.FailureResult($"Locale with ID '{id}' was not found."));

        return OkResponse(locale);
    }

    /// <summary>
    /// Gets a supported locale by its culture code (e.g. en-US, ar-SA, id-ID).
    /// </summary>
    [HttpGet("by-culture/{cultureCode}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<SupportedLanguageResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCultureCode(string cultureCode, CancellationToken cancellationToken = default)
    {
        var locale = await _localeService.GetByCultureCodeAsync(cultureCode, cancellationToken);
        if (locale == null)
            return NotFound(ApiResponse.FailureResult($"Locale with culture code '{cultureCode}' was not found."));

        return OkResponse(locale);
    }
}
