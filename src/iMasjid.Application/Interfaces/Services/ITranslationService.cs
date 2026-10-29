namespace iMasjid.Application.Interfaces.Services;

public interface ITranslationService
{
    Task<Dictionary<Guid, (string Name, string? Description)>> GetTranslationsMapAsync(
        string entityType,
        string cultureCode,
        CancellationToken cancellationToken = default);
}
