using AutoMapper;
using iMasjid.Application.DTOs.Responses.Localization;
using iMasjid.Application.Interfaces.Features;
using iMasjid.Application.Interfaces.Persistence;
using iMasjid.Domain.Entities;

namespace iMasjid.Application.Features.Localization;

public class LocaleService : ILocaleService
{
    private readonly IGenericRepository<ImLocale> _localeRepository;
    private readonly IMapper _mapper;

    public LocaleService(IGenericRepository<ImLocale> localeRepository, IMapper mapper)
    {
        _localeRepository = localeRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<SupportedLanguageResponse>> GetSupportedLanguagesAsync(bool onlyActive = true, CancellationToken cancellationToken = default)
    {
        var locales = onlyActive
            ? await _localeRepository.FindAsync(l => l.IsActive && !l.IsDeleted, cancellationToken)
            : await _localeRepository.FindAsync(l => !l.IsDeleted, cancellationToken);

        var ordered = locales.OrderBy(l => l.DisplayOrder).ThenBy(l => l.LanguageName).ToList();
        return _mapper.Map<IReadOnlyList<SupportedLanguageResponse>>(ordered);
    }

    public async Task<SupportedLanguageResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var locale = await _localeRepository.GetByIdAsync(id, cancellationToken);
        if (locale == null || locale.IsDeleted)
            return null;

        return _mapper.Map<SupportedLanguageResponse>(locale);
    }

    public async Task<SupportedLanguageResponse?> GetByCultureCodeAsync(string cultureCode, CancellationToken cancellationToken = default)
    {
        var normalized = cultureCode.Trim().ToLowerInvariant();
        var locales = await _localeRepository.FindAsync(
            l => l.CultureCode.ToLower() == normalized && !l.IsDeleted,
            cancellationToken);

        var locale = locales.FirstOrDefault();
        return locale == null ? null : _mapper.Map<SupportedLanguageResponse>(locale);
    }
}
