using Microsoft.EntityFrameworkCore;
using iMasjid.Application.Interfaces.Services;
using iMasjid.Infrastructure.Persistence;

namespace iMasjid.Infrastructure.Services;

public class TranslationService : ITranslationService
{
    private readonly AppDbContext _context;
    private readonly ICacheService _cacheService;

    public TranslationService(AppDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<Dictionary<Guid, (string Name, string? Description)>> GetTranslationsMapAsync(
        string entityType,
        string cultureCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cultureCode))
            cultureCode = "id-ID";

        var cacheKey = $"trans_{entityType.ToLowerInvariant()}_{cultureCode.ToLowerInvariant()}";

        return await _cacheService.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                var list = await _context.ImTranslations
                    .AsNoTracking()
                    .Where(t => t.EntityType == entityType && t.CultureCode == cultureCode && !t.IsDeleted)
                    .Select(t => new { t.EntityId, t.Name, t.Description })
                    .ToListAsync(cancellationToken);

                var dict = new Dictionary<Guid, (string Name, string? Description)>();
                foreach (var item in list)
                {
                    dict[item.EntityId] = (item.Name, item.Description);
                }
                return dict;
            },
            TimeSpan.FromMinutes(30),
            cancellationToken: cancellationToken);
    }
}
