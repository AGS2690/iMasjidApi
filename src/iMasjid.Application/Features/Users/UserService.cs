using System.Linq.Expressions;
using AutoMapper;
using iMasjid.Application.Common.Models;
using iMasjid.Application.DTOs.Requests.Users;
using iMasjid.Application.DTOs.Responses.Users;
using iMasjid.Application.Interfaces.Features;
using iMasjid.Application.Interfaces.Persistence;
using iMasjid.Application.Interfaces.Services;
using iMasjid.Domain.Entities;
using iMasjid.Domain.Enums;
using iMasjid.Domain.Exceptions;
using RolesConstants = iMasjid.Domain.Constants.Roles;
using CacheKeys = iMasjid.Domain.Constants.CacheKeys;

namespace iMasjid.Application.Features.Users;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IGenericRepository<ImRole> _roleRepository;
    private readonly IGenericRepository<ImPermission> _permissionRepository;
    private readonly IGenericRepository<ImUserRole> _userRoleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICacheService _cacheService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;
    private readonly ITokenService _tokenService;

    public UserService(
        IUserRepository userRepository,
        IGenericRepository<ImRole> roleRepository,
        IGenericRepository<ImPermission> permissionRepository,
        IGenericRepository<ImUserRole> userRoleRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ICacheService cacheService,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IMapper mapper,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _userRoleRepository = userRoleRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _cacheService = cacheService;
        _currentUserService = currentUserService;
        _auditService = auditService;
        _mapper = mapper;
        _tokenService = tokenService;
    }

    public async Task<UserResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null || user.IsDeleted)
            throw new NotFoundException(nameof(ImUser), id);

        var roles = await _userRepository.GetUserRolesAsync(id, cancellationToken);
        return MapToUserResponse(user, roles);
    }

    public async Task<UserDetailsResponse> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetWithRolesAndPermissionsAsync(id, cancellationToken);
        if (user == null || user.IsDeleted)
            throw new NotFoundException(nameof(ImUser), id);

        var roles = user.ImUserRoles.Select(ur => ur.Role.Name).ToList();
        var permissions = await GetEffectivePermissionsAsync(id, cancellationToken);

        var userResponse = MapToUserResponse(user, roles);
        return new UserDetailsResponse
        {
            User = userResponse,
            Roles = roles,
            Permissions = permissions
        };
    }

    public async Task<PagedResponse<UserResponse>> GetPagedListAsync(GetUsersRequest request, CancellationToken cancellationToken = default)
    {
        Expression<Func<ImUser, bool>> predicate = u => !u.IsDeleted
            && (!request.IsActive.HasValue || u.IsActive == request.IsActive.Value)
            && (string.IsNullOrWhiteSpace(request.SearchTerm) ||
                u.FirstName.Contains(request.SearchTerm) ||
                u.LastName.Contains(request.SearchTerm) ||
                u.Email.Contains(request.SearchTerm) ||
                u.UserName.Contains(request.SearchTerm));

        var pagedUsers = await _userRepository.GetPagedListAsync(
            request,
            predicate,
            q => q.OrderByDescending(u => u.CreatedOn),
            cancellationToken);

        if (pagedUsers.Items.Count == 0)
        {
            return PagedResponse<UserResponse>.Create(
                Array.Empty<UserResponse>(),
                pagedUsers.TotalCount,
                pagedUsers.PageNumber,
                pagedUsers.PageSize);
        }

        var userIds = pagedUsers.Items.Select(u => u.Id).ToList();

        var rolesByUserId = await _userRepository.GetRolesForUsersAsync(userIds, cancellationToken);

        var userResponses = pagedUsers.Items.Select(user =>
        {
            var roles = rolesByUserId.TryGetValue(user.Id, out var r) ? r : Array.Empty<string>();
            return MapToUserResponse(user, roles);
        }).ToList();

        return PagedResponse<UserResponse>.Create(
            userResponses,
            pagedUsers.TotalCount,
            pagedUsers.PageNumber,
            pagedUsers.PageSize);
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (!await _userRepository.IsEmailUniqueAsync(request.Email, null, cancellationToken))
            throw new ConflictException($"User with email '{request.Email}' already exists.");

        if (!await _userRepository.IsUserNameUniqueAsync(request.UserName, null, cancellationToken))
            throw new ConflictException($"User with username '{request.UserName}' already exists.");

        var user = new ImUser
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            UserName = request.UserName.Trim().ToLowerInvariant(),
            Email = request.Email.Trim().ToLowerInvariant(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            IsActive = true,
            IsEmailVerified = false,
            CreatedBy = _currentUserService.UserName
        };

        var roleGuids = new HashSet<Guid>();
        var roleNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (request.RoleIds != null)
        {
            foreach (var rid in request.RoleIds)
                roleGuids.Add(rid);
        }

        if (request.Roles != null)
        {
            foreach (var r in request.Roles)
            {
                if (Guid.TryParse(r, out var parsedGuid))
                    roleGuids.Add(parsedGuid);
                else if (!string.IsNullOrWhiteSpace(r))
                    roleNames.Add(r.Trim());
            }
        }

        if (roleGuids.Count == 0 && roleNames.Count == 0)
        {
            roleNames.Add(RolesConstants.User);
        }

        var matchedRoles = new List<ImRole>();
        if (roleGuids.Count > 0)
        {
            var rolesById = await _roleRepository.FindAsync(r => roleGuids.Contains(r.Id) && !r.IsDeleted, cancellationToken);
            matchedRoles.AddRange(rolesById);
        }

        if (roleNames.Count > 0)
        {
            var normalizedNames = roleNames.Select(n => n.ToUpperInvariant()).ToList();
            var rolesByName = await _roleRepository.FindAsync(r => normalizedNames.Contains(r.NormalizedName) && !r.IsDeleted, cancellationToken);
            foreach (var role in rolesByName)
            {
                if (!matchedRoles.Any(r => r.Id == role.Id))
                    matchedRoles.Add(role);
            }
        }

        var assignedRoles = new List<string>();
        foreach (var role in matchedRoles)
        {
            user.ImUserRoles.Add(new ImUserRole
            {
                RoleId = role.Id,
                CreatedBy = _currentUserService.UserName
            });
            assignedRoles.Add(role.Name);
        }

        await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await _userRepository.AddAsync(user, cancellationToken);
        }, cancellationToken);

        await _auditService.LogAsync(
            _currentUserService.UserId?.ToString(),
            AuditAction.Create,
            nameof(ImUser),
            user.Id.ToString(),
            null,
            new { user.UserName, user.Email, Roles = assignedRoles },
            _currentUserService.IpAddress,
            _currentUserService.UserAgent,
            cancellationToken);

        return MapToUserResponse(user, assignedRoles);
    }

    public async Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null || user.IsDeleted)
            throw new NotFoundException(nameof(ImUser), id);

        var oldValues = new { user.FirstName, user.LastName, user.PhoneNumber };

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.PhoneNumber = request.PhoneNumber?.Trim();
        user.ModifiedBy = _currentUserService.UserName;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            _currentUserService.UserId?.ToString(),
            AuditAction.Update,
            nameof(ImUser),
            user.Id.ToString(),
            oldValues,
            new { user.FirstName, user.LastName, user.PhoneNumber },
            _currentUserService.IpAddress,
            _currentUserService.UserAgent,
            cancellationToken);

        var roles = await _userRepository.GetUserRolesAsync(id, cancellationToken);
        return MapToUserResponse(user, roles);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null || user.IsDeleted)
            throw new NotFoundException(nameof(ImUser), id);

        user.IsDeleted = true;

        _userRepository.Update(user);
        await _tokenService.RevokeAllUserTokensAsync(id, _currentUserService.IpAddress, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync(CacheKeys.UserPermissions(id), cancellationToken);

        await _auditService.LogAsync(
            _currentUserService.UserId?.ToString(),
            AuditAction.Delete,
            nameof(ImUser),
            id.ToString(),
            new { user.UserName, user.Email },
            null,
            _currentUserService.IpAddress,
            _currentUserService.UserAgent,
            cancellationToken);

        return true;
    }

    public async Task<bool> ToggleStatusAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null || user.IsDeleted)
            throw new NotFoundException(nameof(ImUser), id);

        user.IsActive = !user.IsActive;
        user.ModifiedBy = _currentUserService.UserName;

        _userRepository.Update(user);
        if (!user.IsActive)
        {
            await _tokenService.RevokeAllUserTokensAsync(id, _currentUserService.IpAddress, cancellationToken);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            _currentUserService.UserId?.ToString(),
            AuditAction.Update,
            nameof(ImUser),
            id.ToString(),
            new { IsActive = !user.IsActive },
            new { user.IsActive },
            _currentUserService.IpAddress,
            _currentUserService.UserAgent,
            cancellationToken);

        return true;
    }

    public async Task<bool> AssignRolesAsync(Guid id, AssignUserRolesRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null || user.IsDeleted)
            throw new NotFoundException(nameof(ImUser), id);

        var roleGuids = new HashSet<Guid>();
        var roleNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (request.RoleIds != null)
        {
            foreach (var rid in request.RoleIds)
                roleGuids.Add(rid);
        }

        if (request.Roles != null)
        {
            foreach (var r in request.Roles)
            {
                if (Guid.TryParse(r, out var parsedGuid))
                    roleGuids.Add(parsedGuid);
                else if (!string.IsNullOrWhiteSpace(r))
                    roleNames.Add(r.Trim());
            }
        }

        var matchedRoles = new List<ImRole>();
        if (roleGuids.Count > 0)
        {
            var rolesById = await _roleRepository.FindAsync(r => roleGuids.Contains(r.Id) && !r.IsDeleted, cancellationToken);
            matchedRoles.AddRange(rolesById);
        }

        if (roleNames.Count > 0)
        {
            var normalizedNames = roleNames.Select(n => n.ToUpperInvariant()).ToList();
            var rolesByName = await _roleRepository.FindAsync(r => normalizedNames.Contains(r.NormalizedName) && !r.IsDeleted, cancellationToken);
            foreach (var role in rolesByName)
            {
                if (!matchedRoles.Any(r => r.Id == role.Id))
                    matchedRoles.Add(role);
            }
        }

        var existingUserRoles = await _userRoleRepository.FindAsync(ur => ur.UserId == id, cancellationToken);

        await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            _userRoleRepository.DeleteRange(existingUserRoles);

            foreach (var role in matchedRoles)
            {
                await _userRoleRepository.AddAsync(new ImUserRole
                {
                    UserId = id,
                    RoleId = role.Id,
                    CreatedBy = _currentUserService.UserName
                }, cancellationToken);
            }
        }, cancellationToken);

        await _cacheService.RemoveAsync(CacheKeys.UserPermissions(id), cancellationToken);

        await _auditService.LogAsync(
            _currentUserService.UserId?.ToString(),
            AuditAction.Update,
            nameof(ImUser),
            id.ToString(),
            new { Roles = existingUserRoles.Select(r => r.RoleId) },
            new { Roles = matchedRoles.Select(r => r.Name) },
            _currentUserService.IpAddress,
            _currentUserService.UserAgent,
            cancellationToken);

        return true;
    }

    public async Task<IReadOnlyList<string>> GetEffectivePermissionsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeys.UserPermissions(id);
        return await _cacheService.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                return await _userRepository.GetUserPermissionsAsync(id, cancellationToken);
            },
            TimeSpan.FromMinutes(15),
            TimeSpan.FromMinutes(5),
            cancellationToken);
    }

    private UserResponse MapToUserResponse(ImUser user, IReadOnlyList<string> roles)
        => _mapper.Map<UserResponse>(user, opts => opts.Items["Roles"] = roles);
}
