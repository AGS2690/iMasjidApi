using AutoMapper;
using iMasjid.Application.DTOs.Requests.Roles;
using iMasjid.Application.DTOs.Responses.Roles;
using iMasjid.Application.Interfaces.Features;
using iMasjid.Application.Interfaces.Persistence;
using iMasjid.Application.Interfaces.Services;
using iMasjid.Domain.Entities;
using iMasjid.Domain.Enums;
using iMasjid.Domain.Exceptions;
using RolesConstants = iMasjid.Domain.Constants.Roles;
using CacheKeys = iMasjid.Domain.Constants.CacheKeys;

namespace iMasjid.Application.Features.Roles;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IGenericRepository<ImPermission> _permissionRepository;
    private readonly IGenericRepository<ImRolePermission> _rolePermissionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITranslationService _translationService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public RoleService(
        IRoleRepository roleRepository,
        IGenericRepository<ImPermission> permissionRepository,
        IGenericRepository<ImRolePermission> rolePermissionRepository,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ICurrentUserService currentUserService,
        ITranslationService translationService,
        IAuditService auditService,
        IMapper mapper)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _rolePermissionRepository = rolePermissionRepository;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _currentUserService = currentUserService;
        _translationService = translationService;
        _auditService = auditService;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<RoleResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _roleRepository.GetAllWithPermissionsAsync(cancellationToken);
        return roles.Select(MapToRoleResponse).ToList();
    }

    public async Task<RoleResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdWithPermissionsAsync(id, cancellationToken);
        if (role == null || role.IsDeleted)
            throw new NotFoundException(nameof(ImRole), id);

        return MapToRoleResponse(role);
    }

    public async Task<RoleResponse> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedName = request.Name.Trim().ToUpperInvariant();
        var exists = await _roleRepository.AnyAsync(r => r.NormalizedName == normalizedName && !r.IsDeleted, cancellationToken);
        if (exists)
            throw new ConflictException($"Role with name '{request.Name}' already exists.");

        var role = new ImRole
        {
            Name = request.Name.Trim(),
            NormalizedName = normalizedName,
            Description = request.Description?.Trim(),
            IsActive = true,
            CreatedBy = _currentUserService.UserName
        };

        var assignedPermCodes = new List<string>();
        if (request.Permissions != null && request.Permissions.Count > 0)
        {
            var permissions = await _permissionRepository.FindAsync(p => request.Permissions.Contains(p.Code), cancellationToken);
            foreach (var perm in permissions)
            {
                role.ImRolePermissions.Add(new ImRolePermission
                {
                    PermissionId = perm.Id,
                    CreatedBy = _currentUserService.UserName
                });
                assignedPermCodes.Add(perm.Code);
            }
        }

        await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await _roleRepository.AddAsync(role, cancellationToken);
        }, cancellationToken);

        await _auditService.LogAsync(
            _currentUserService.UserId?.ToString(),
            AuditAction.Create,
            nameof(ImRole),
            role.Id.ToString(),
            null,
            new { role.Name, Permissions = assignedPermCodes },
            _currentUserService.IpAddress,
            _currentUserService.UserAgent,
            cancellationToken);

        return new RoleResponse
        {
            Id = role.Id,
            Name = role.Name,
            NormalizedName = role.NormalizedName,
            Description = role.Description,
            IsActive = role.IsActive,
            CreatedOn = role.CreatedOn,
            Permissions = assignedPermCodes
        };
    }

    public async Task<RoleResponse> UpdateAsync(Guid id, UpdateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(id, cancellationToken);
        if (role == null || role.IsDeleted)
            throw new NotFoundException(nameof(ImRole), id);

        role.Description = request.Description?.Trim();
        role.ModifiedBy = _currentUserService.UserName;

        _roleRepository.Update(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(id, cancellationToken);
        if (role == null || role.IsDeleted)
            throw new NotFoundException(nameof(ImRole), id);

        if (role.NormalizedName == RolesConstants.SuperAdmin.ToUpperInvariant())
            throw new DomainException("SuperAdmin role cannot be deleted.");

        role.IsDeleted = true;

        _roleRepository.Update(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync(CacheKeys.RolePermissions(id), cancellationToken);

        return true;
    }

    public async Task<bool> UpdatePermissionsAsync(Guid id, UpdateRolePermissionsRequest request, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(id, cancellationToken);
        if (role == null || role.IsDeleted)
            throw new NotFoundException(nameof(ImRole), id);

        var existingPerms = await _rolePermissionRepository.FindAsync(rp => rp.RoleId == id, cancellationToken);
        var permissions = await _permissionRepository.FindAsync(p => request.Permissions.Contains(p.Code), cancellationToken);

        await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            _rolePermissionRepository.DeleteRange(existingPerms);

            foreach (var perm in permissions)
            {
                await _rolePermissionRepository.AddAsync(new ImRolePermission
                {
                    RoleId = id,
                    PermissionId = perm.Id,
                    CreatedBy = _currentUserService.UserName
                }, cancellationToken);
            }
        }, cancellationToken);

        await _cacheService.RemoveAsync(CacheKeys.RolePermissions(id), cancellationToken);

        return true;
    }

    public async Task<IReadOnlyList<PermissionDto>> GetAllPermissionsAsync(CancellationToken cancellationToken = default)
    {
        var permissions = await _permissionRepository.GetAllAsync(cancellationToken);
        var culture = _currentUserService.CultureCode;
        var permTrans = await _translationService.GetTranslationsMapAsync("Permission", culture, cancellationToken);

        return permissions
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Name)
            .Select(p =>
            {
                var name = permTrans.TryGetValue(p.Id, out var pt) && !string.IsNullOrWhiteSpace(pt.Name)
                    ? pt.Name
                    : p.Name;
                var desc = permTrans.TryGetValue(p.Id, out var ptDesc) && ptDesc.Description != null
                    ? ptDesc.Description
                    : p.Description;

                return new PermissionDto
                {
                    Id = p.Id,
                    Name = name,
                    Code = p.Code,
                    Description = desc,
                    Module = p.Module,
                    IsActive = p.IsActive
                };
            })
            .ToList();
    }

    private static RoleResponse MapToRoleResponse(ImRole role)
    {
        return new RoleResponse
        {
            Id = role.Id,
            Name = role.Name,
            NormalizedName = role.NormalizedName,
            Description = role.Description,
            IsActive = role.IsActive,
            CreatedOn = role.CreatedOn,
            Permissions = role.ImRolePermissions != null
                ? role.ImRolePermissions
                    .Where(rp => rp != null && rp.Permission != null && rp.Permission.IsActive && !rp.Permission.IsDeleted)
                    .Select(rp => rp.Permission.Code)
                    .Distinct()
                    .ToList()
                : new List<string>()
        };
    }
}
