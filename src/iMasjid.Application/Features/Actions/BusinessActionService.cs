using AutoMapper;
using iMasjid.Application.DTOs.Requests.Actions;
using iMasjid.Application.DTOs.Responses.Actions;
using iMasjid.Application.Interfaces.Features;
using iMasjid.Application.Interfaces.Persistence;
using iMasjid.Application.Interfaces.Services;
using iMasjid.Domain.Entities;
using iMasjid.Domain.Enums;
using iMasjid.Domain.Exceptions;

namespace iMasjid.Application.Features.Actions;

public class BusinessActionService : IBusinessActionService
{
    private readonly IGenericRepository<ImBusinessAction> _actionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITranslationService _translationService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public BusinessActionService(
        IGenericRepository<ImBusinessAction> actionRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ITranslationService translationService,
        IAuditService auditService,
        IMapper mapper)
    {
        _actionRepository = actionRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _translationService = translationService;
        _auditService = auditService;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<BusinessActionResponse>> GetAllAsync(bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var actions = await _actionRepository.FindAsync(
            a => !a.IsDeleted && (!isActive.HasValue || a.IsActive == isActive.Value),
            cancellationToken);

        var list = _mapper.Map<IReadOnlyList<BusinessActionResponse>>(
            actions.OrderBy(a => a.Name).ToList());

        var culture = _currentUserService.CultureCode;
        var actionTrans = await _translationService.GetTranslationsMapAsync("BusinessAction", culture, cancellationToken);

        foreach (var item in list)
        {
            if (actionTrans.TryGetValue(item.Id, out var at))
            {
                if (!string.IsNullOrWhiteSpace(at.Name)) item.Name = at.Name;
                if (at.Description != null) item.Description = at.Description;
            }
        }

        return list;
    }

    public async Task<BusinessActionResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var action = await _actionRepository.GetByIdAsync(id, cancellationToken);
        if (action == null || action.IsDeleted)
            throw new NotFoundException(nameof(ImBusinessAction), id);

        var res = _mapper.Map<BusinessActionResponse>(action);

        var culture = _currentUserService.CultureCode;
        var actionTrans = await _translationService.GetTranslationsMapAsync("BusinessAction", culture, cancellationToken);

        if (actionTrans.TryGetValue(res.Id, out var at))
        {
            if (!string.IsNullOrWhiteSpace(at.Name)) res.Name = at.Name;
            if (at.Description != null) res.Description = at.Description;
        }

        return res;
    }

    public async Task<BusinessActionResponse> CreateAsync(CreateBusinessActionRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        var existing = await _actionRepository.FindAsync(a => a.Code == normalizedCode && !a.IsDeleted, cancellationToken);
        if (existing.Count > 0)
            throw new ConflictException($"Business action with code '{normalizedCode}' already exists.");

        var action = new ImBusinessAction
        {
            Name = request.Name.Trim(),
            Code = normalizedCode,
            Description = request.Description?.Trim(),
            IsActive = request.IsActive,
            CreatedBy = _currentUserService.UserName
        };

        await _actionRepository.AddAsync(action, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            _currentUserService.UserId?.ToString(),
            AuditAction.Create,
            nameof(ImBusinessAction),
            action.Id.ToString(),
            null,
            new { action.Name, action.Code },
            _currentUserService.IpAddress,
            _currentUserService.UserAgent,
            cancellationToken);

        return _mapper.Map<BusinessActionResponse>(action);
    }

    public async Task<BusinessActionResponse> UpdateAsync(Guid id, UpdateBusinessActionRequest request, CancellationToken cancellationToken = default)
    {
        var action = await _actionRepository.GetByIdAsync(id, cancellationToken);
        if (action == null || action.IsDeleted)
            throw new NotFoundException(nameof(ImBusinessAction), id);

        var oldValues = new { action.Name, action.Description, action.IsActive };

        action.Name = request.Name.Trim();
        action.Description = request.Description?.Trim();
        action.IsActive = request.IsActive;
        action.ModifiedBy = _currentUserService.UserName;

        _actionRepository.Update(action);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            _currentUserService.UserId?.ToString(),
            AuditAction.Update,
            nameof(ImBusinessAction),
            action.Id.ToString(),
            oldValues,
            new { action.Name, action.Description, action.IsActive },
            _currentUserService.IpAddress,
            _currentUserService.UserAgent,
            cancellationToken);

        return _mapper.Map<BusinessActionResponse>(action);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var action = await _actionRepository.GetByIdAsync(id, cancellationToken);
        if (action == null || action.IsDeleted)
            throw new NotFoundException(nameof(ImBusinessAction), id);

        action.IsDeleted = true;

        _actionRepository.Update(action);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            _currentUserService.UserId?.ToString(),
            AuditAction.Delete,
            nameof(ImBusinessAction),
            id.ToString(),
            new { action.Name, action.Code },
            null,
            _currentUserService.IpAddress,
            _currentUserService.UserAgent,
            cancellationToken);

        return true;
    }
}
