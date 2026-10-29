using iMasjid.Application.DTOs.Requests.Settings;
using iMasjid.Application.DTOs.Responses.Settings;

namespace iMasjid.Application.Interfaces.Features;

public interface IApplicationSettingService
{
    Task<IReadOnlyList<SettingResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SettingResponse> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<string?> GetValueAsync(string key, CancellationToken cancellationToken = default);
    Task<SettingResponse> CreateAsync(CreateSettingRequest request, CancellationToken cancellationToken = default);
    Task<SettingResponse> UpdateAsync(string key, UpdateSettingRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);
}
