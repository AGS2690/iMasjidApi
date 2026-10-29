using iMasjid.Application.DTOs.Responses.Localization;

namespace iMasjid.Application.Interfaces.Features;

public interface ILocaleService
{
    Task<IReadOnlyList<SupportedLanguageResponse>> GetSupportedLanguagesAsync(bool onlyActive = true, CancellationToken cancellationToken = default);
    Task<SupportedLanguageResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SupportedLanguageResponse?> GetByCultureCodeAsync(string cultureCode, CancellationToken cancellationToken = default);
}
