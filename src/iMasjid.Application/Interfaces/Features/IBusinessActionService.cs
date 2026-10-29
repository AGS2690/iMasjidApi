using iMasjid.Application.DTOs.Requests.Actions;
using iMasjid.Application.DTOs.Responses.Actions;

namespace iMasjid.Application.Interfaces.Features;

public interface IBusinessActionService
{
    Task<IReadOnlyList<BusinessActionResponse>> GetAllAsync(bool? isActive = null, CancellationToken cancellationToken = default);
    Task<BusinessActionResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BusinessActionResponse> CreateAsync(CreateBusinessActionRequest request, CancellationToken cancellationToken = default);
    Task<BusinessActionResponse> UpdateAsync(Guid id, UpdateBusinessActionRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
