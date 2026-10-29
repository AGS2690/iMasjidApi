using AutoMapper;
using iMasjid.Application.DTOs.Requests.Settings;
using iMasjid.Application.DTOs.Responses.Settings;
using iMasjid.Application.Interfaces.Features;
using iMasjid.Application.Interfaces.Persistence;
using iMasjid.Application.Interfaces.Services;
using iMasjid.Domain.Constants;
using iMasjid.Domain.Entities;
using iMasjid.Domain.Exceptions;

namespace iMasjid.Application.Features.Settings;

public class ApplicationSettingService : IApplicationSettingService
{
    private readonly IGenericRepository<ImApplicationSetting> _settingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public ApplicationSettingService(
        IGenericRepository<ImApplicationSetting> settingRepository,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _settingRepository = settingRepository;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<SettingResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _settingRepository.FindAsync(s => !s.IsDeleted, cancellationToken);
        return _mapper.Map<IReadOnlyList<SettingResponse>>(settings.ToList());
    }

    public async Task<SettingResponse> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        var normalizedKey = key.Trim().ToUpperInvariant();
        var settings = await _settingRepository.FindAsync(s => s.Key.ToUpper() == normalizedKey && !s.IsDeleted, cancellationToken);
        var setting = settings.FirstOrDefault();

        if (setting == null)
            throw new NotFoundException($"Application setting '{key}' was not found.");

        return _mapper.Map<SettingResponse>(setting);
    }

    public async Task<string?> GetValueAsync(string key, CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeys.Setting(key);
        return await _cacheService.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                var normalizedKey = key.Trim().ToUpperInvariant();
                var settings = await _settingRepository.FindAsync(s => s.Key.ToUpper() == normalizedKey && !s.IsDeleted, cancellationToken);
                return settings.FirstOrDefault()?.Value;
            },
            TimeSpan.FromHours(1),
            TimeSpan.FromMinutes(10),
            cancellationToken);
    }

    public async Task<SettingResponse> CreateAsync(CreateSettingRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedKey = request.Key.Trim().ToUpperInvariant();
        var exists = await _settingRepository.AnyAsync(s => s.Key.ToUpper() == normalizedKey && !s.IsDeleted, cancellationToken);
        if (exists)
            throw new ConflictException($"Setting with key '{request.Key}' already exists.");

        var setting = new ImApplicationSetting
        {
            Key = request.Key.Trim(),
            Value = request.Value,
            Description = request.Description?.Trim(),
            Module = request.Module.Trim(),
            IsEncrypted = request.IsEncrypted,
            CreatedBy = _currentUserService.UserName
        };

        await _settingRepository.AddAsync(setting, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cacheService.SetAsync(CacheKeys.Setting(setting.Key), setting.Value, cancellationToken: cancellationToken);

        return _mapper.Map<SettingResponse>(setting);
    }

    public async Task<SettingResponse> UpdateAsync(string key, UpdateSettingRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedKey = key.Trim().ToUpperInvariant();
        var settings = await _settingRepository.FindAsync(s => s.Key.ToUpper() == normalizedKey && !s.IsDeleted, cancellationToken);
        var setting = settings.FirstOrDefault();

        if (setting == null)
            throw new NotFoundException($"Application setting '{key}' was not found.");

        setting.Value = request.Value;
        setting.Description = request.Description?.Trim();
        setting.ModifiedBy = _currentUserService.UserName;

        _settingRepository.Update(setting);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync(CacheKeys.Setting(setting.Key), cancellationToken);

        return _mapper.Map<SettingResponse>(setting);
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        var normalizedKey = key.Trim().ToUpperInvariant();
        var settings = await _settingRepository.FindAsync(s => s.Key.ToUpper() == normalizedKey && !s.IsDeleted, cancellationToken);
        var setting = settings.FirstOrDefault();

        if (setting == null)
            throw new NotFoundException($"Application setting '{key}' was not found.");

        _settingRepository.Delete(setting);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync(CacheKeys.Setting(setting.Key), cancellationToken);
    }
}
