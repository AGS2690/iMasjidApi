using AutoMapper;
using iMasjid.Application.DTOs.Requests.Menus;
using iMasjid.Application.DTOs.Responses.Actions;
using iMasjid.Application.DTOs.Responses.Menus;
using iMasjid.Application.Interfaces.Features;
using iMasjid.Application.Interfaces.Persistence;
using iMasjid.Application.Interfaces.Services;
using iMasjid.Domain.Entities;
using iMasjid.Domain.Enums;
using iMasjid.Domain.Exceptions;
using RolesConstants = iMasjid.Domain.Constants.Roles;

namespace iMasjid.Application.Features.Menus;

public class MenuService : IMenuService
{
    private readonly IMenuRepository _menuRepository;
    private readonly IGenericRepository<ImBusinessAction> _actionRepository;
    private readonly IGenericRepository<ImMenuAction> _menuActionRepository;
    private readonly IGenericRepository<ImRoleMenu> _roleMenuRepository;
    private readonly IGenericRepository<ImRoleMenuAction> _roleMenuActionRepository;
    private readonly IGenericRepository<ImUserRole> _userRoleRepository;
    private readonly IGenericRepository<ImRole> _roleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITranslationService _translationService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public MenuService(
        IMenuRepository menuRepository,
        IGenericRepository<ImBusinessAction> actionRepository,
        IGenericRepository<ImMenuAction> menuActionRepository,
        IGenericRepository<ImRoleMenu> roleMenuRepository,
        IGenericRepository<ImRoleMenuAction> roleMenuActionRepository,
        IGenericRepository<ImUserRole> userRoleRepository,
        IGenericRepository<ImRole> roleRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ITranslationService translationService,
        IAuditService auditService,
        IMapper mapper)
    {
        _menuRepository = menuRepository;
        _actionRepository = actionRepository;
        _menuActionRepository = menuActionRepository;
        _roleMenuRepository = roleMenuRepository;
        _roleMenuActionRepository = roleMenuActionRepository;
        _userRoleRepository = userRoleRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _translationService = translationService;
        _auditService = auditService;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<MenuResponse>> GetAllAsync(bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var menus = await _menuRepository.GetAllWithActionsAsync(isActive, cancellationToken);
        var culture = _currentUserService.CultureCode;
        var menuTrans = await _translationService.GetTranslationsMapAsync("Menu", culture, cancellationToken);
        var actionTrans = await _translationService.GetTranslationsMapAsync("BusinessAction", culture, cancellationToken);

        return menus.Select(m => MapToResponse(m, menuTrans, actionTrans)).ToList();
    }

    public async Task<IReadOnlyList<MenuTreeNodeResponse>> GetTreeAsync(bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var allMenus = await _menuRepository.GetAllWithActionsAsync(isActive, cancellationToken);
        var culture = _currentUserService.CultureCode;
        var menuTrans = await _translationService.GetTranslationsMapAsync("Menu", culture, cancellationToken);
        var actionTrans = await _translationService.GetTranslationsMapAsync("BusinessAction", culture, cancellationToken);

        return BuildMenuTree(allMenus.ToList(), null, menuTrans, actionTrans);
    }

    public async Task<MenuResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var menu = await _menuRepository.GetByIdWithActionsAsync(id, cancellationToken);
        if (menu == null)
            throw new NotFoundException(nameof(ImMenu), id);

        var culture = _currentUserService.CultureCode;
        var menuTrans = await _translationService.GetTranslationsMapAsync("Menu", culture, cancellationToken);
        var actionTrans = await _translationService.GetTranslationsMapAsync("BusinessAction", culture, cancellationToken);

        return MapToResponse(menu, menuTrans, actionTrans);
    }

    public async Task<MenuResponse> CreateAsync(CreateMenuRequest request, CancellationToken cancellationToken = default)
    {
        if (request.ParentId.HasValue)
        {
            var parentExists = await _menuRepository.AnyAsync(m => m.Id == request.ParentId.Value && !m.IsDeleted, cancellationToken);
            if (!parentExists)
                throw new NotFoundException($"Parent menu with ID '{request.ParentId.Value}' was not found.");
        }

        var menu = new ImMenu
        {
            Title = request.Title.Trim(),
            Route = request.Route?.Trim(),
            Icon = request.Icon?.Trim(),
            ParentId = request.ParentId,
            OrderIndex = request.OrderIndex,
            IsActive = request.IsActive,
            CreatedBy = _currentUserService.UserName
        };

        // Map initial actions if provided
        if (request.ActionIds != null && request.ActionIds.Count > 0)
        {
            foreach (var actionId in request.ActionIds)
            {
                var actionExists = await _actionRepository.AnyAsync(a => a.Id == actionId && !a.IsDeleted, cancellationToken);
                if (actionExists)
                {
                    menu.ImMenuActions.Add(new ImMenuAction
                    {
                        ActionId = actionId,
                        IsActive = true,
                        CreatedBy = _currentUserService.UserName
                    });
                }
            }
        }

        await _menuRepository.AddAsync(menu, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            _currentUserService.UserId?.ToString(),
            AuditAction.Create,
            nameof(ImMenu),
            menu.Id.ToString(),
            null,
            new { menu.Title, menu.Route, menu.ParentId },
            _currentUserService.IpAddress,
            _currentUserService.UserAgent,
            cancellationToken);

        return await GetByIdAsync(menu.Id, cancellationToken);
    }

    public async Task<MenuResponse> UpdateAsync(Guid id, UpdateMenuRequest request, CancellationToken cancellationToken = default)
    {
        var menu = await _menuRepository.GetByIdAsync(id, cancellationToken);
        if (menu == null || menu.IsDeleted)
            throw new NotFoundException(nameof(ImMenu), id);

        if (request.ParentId.HasValue)
        {
            if (request.ParentId.Value == id)
                throw new ValidationException("A menu item cannot be its own parent.");

            var parentExists = await _menuRepository.AnyAsync(m => m.Id == request.ParentId.Value && !m.IsDeleted, cancellationToken);
            if (!parentExists)
                throw new NotFoundException($"Parent menu with ID '{request.ParentId.Value}' was not found.");
        }

        var oldValues = new { menu.Title, menu.Route, menu.Icon, menu.ParentId, menu.OrderIndex, menu.IsActive };

        menu.Title = request.Title.Trim();
        menu.Route = request.Route?.Trim();
        menu.Icon = request.Icon?.Trim();
        menu.ParentId = request.ParentId;
        menu.OrderIndex = request.OrderIndex;
        menu.IsActive = request.IsActive;
        menu.ModifiedBy = _currentUserService.UserName;

        _menuRepository.Update(menu);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            _currentUserService.UserId?.ToString(),
            AuditAction.Update,
            nameof(ImMenu),
            menu.Id.ToString(),
            oldValues,
            new { menu.Title, menu.Route, menu.Icon, menu.ParentId, menu.OrderIndex, menu.IsActive },
            _currentUserService.IpAddress,
            _currentUserService.UserAgent,
            cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var menu = await _menuRepository.GetByIdAsync(id, cancellationToken);
        if (menu == null || menu.IsDeleted)
            throw new NotFoundException(nameof(ImMenu), id);

        // Cascade soft-delete to children
        var children = await _menuRepository.FindAsync(m => m.ParentId == id && !m.IsDeleted, cancellationToken);
        foreach (var child in children)
        {
            child.IsDeleted = true;
            _menuRepository.Update(child);
        }

        menu.IsDeleted = true;

        _menuRepository.Update(menu);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            _currentUserService.UserId?.ToString(),
            AuditAction.Delete,
            nameof(ImMenu),
            id.ToString(),
            new { menu.Title, menu.Route },
            null,
            _currentUserService.IpAddress,
            _currentUserService.UserAgent,
            cancellationToken);

        return true;
    }

    public async Task<bool> AssignMenuActionsAsync(Guid menuId, AssignMenuActionsRequest request, CancellationToken cancellationToken = default)
    {
        var menuExists = await _menuRepository.AnyAsync(m => m.Id == menuId && !m.IsDeleted, cancellationToken);
        if (!menuExists)
            throw new NotFoundException(nameof(ImMenu), menuId);

        var existingActions = await _menuActionRepository.FindAsync(ma => ma.MenuId == menuId, cancellationToken);

        await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            _menuActionRepository.DeleteRange(existingActions);

            foreach (var actionId in request.ActionIds)
            {
                var actionExists = await _actionRepository.AnyAsync(a => a.Id == actionId && !a.IsDeleted, cancellationToken);
                if (actionExists)
                {
                    await _menuActionRepository.AddAsync(new ImMenuAction
                    {
                        MenuId = menuId,
                        ActionId = actionId,
                        IsActive = true,
                        CreatedBy = _currentUserService.UserName
                    }, cancellationToken);
                }
            }
        }, cancellationToken);

        return true;
    }

    public async Task<IReadOnlyList<UserNavMenuResponse>> GetUserNavigationMenuAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userRoles = await _userRoleRepository.FindAsync(ur => ur.UserId == userId, cancellationToken);
        if (userRoles.Count == 0)
            return Array.Empty<UserNavMenuResponse>();

        var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
        var roles = await _roleRepository.FindAsync(r => roleIds.Contains(r.Id) && r.IsActive && !r.IsDeleted, cancellationToken);

        if (roles.Count == 0)
            return Array.Empty<UserNavMenuResponse>();

        var isSuperAdmin = roles.Any(r => r.NormalizedName == RolesConstants.SuperAdmin.ToUpperInvariant());
        var culture = _currentUserService.CultureCode;
        var menuTrans = await _translationService.GetTranslationsMapAsync("Menu", culture, cancellationToken);
        var actionTrans = await _translationService.GetTranslationsMapAsync("BusinessAction", culture, cancellationToken);

        if (isSuperAdmin)
        {
            // SuperAdmin has full visibility to all active menus and all menu actions
            var allActiveMenus = await _menuRepository.GetAllWithActionsAsync(true, cancellationToken);
            return BuildUserNavTree(allActiveMenus.ToList(), null, null, menuTrans);
        }

        // Retrieve assigned RoleMenus for the user's roles
        var activeRoleIds = roles.Select(r => r.Id).ToList();
        var assignedRoleMenus = await _menuRepository.GetRoleMenusByRoleIdsAsync(activeRoleIds, cancellationToken);

        // Group actions by menu across roles
        var menuActionMap = new Dictionary<Guid, HashSet<string>>();
        var distinctMenus = new Dictionary<Guid, ImMenu>();

        foreach (var rm in assignedRoleMenus)
        {
            if (!distinctMenus.ContainsKey(rm.MenuId))
                distinctMenus[rm.MenuId] = rm.Menu;

            if (!menuActionMap.ContainsKey(rm.MenuId))
                menuActionMap[rm.MenuId] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var rma in rm.ImRoleMenuActions)
            {
                if (rma.MenuAction?.Action?.Code != null)
                {
                    menuActionMap[rm.MenuId].Add(rma.MenuAction.Action.Code);
                }
            }
        }

        // Include parent menus if a child is visible
        var menusToInclude = distinctMenus.Values.ToList();
        var parentIdsToFetch = menusToInclude
            .Where(m => m.ParentId.HasValue && !distinctMenus.ContainsKey(m.ParentId.Value))
            .Select(m => m.ParentId!.Value)
            .Distinct()
            .ToList();

        if (parentIdsToFetch.Count > 0)
        {
            var parentMenus = await _menuRepository.FindAsync(
                m => parentIdsToFetch.Contains(m.Id) && !m.IsDeleted && m.IsActive,
                cancellationToken);

            foreach (var parent in parentMenus)
            {
                if (!distinctMenus.ContainsKey(parent.Id))
                {
                    distinctMenus[parent.Id] = parent;
                    menuActionMap[parent.Id] = new HashSet<string>();
                }
            }
        }

        var sortedMenus = distinctMenus.Values.OrderBy(m => m.OrderIndex).ToList();
        return BuildUserNavTree(sortedMenus, null, menuActionMap, menuTrans);
    }

    public async Task<IReadOnlyList<RoleMenuDetailsResponse>> GetRoleMenusAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var roleExists = await _roleRepository.AnyAsync(r => r.Id == roleId && !r.IsDeleted, cancellationToken);
        if (!roleExists)
            throw new NotFoundException(nameof(ImRole), roleId);

        var allMenus = await _menuRepository.GetAllWithActionsAsync(null, cancellationToken);
        var roleMenus = await _menuRepository.GetRoleMenusByRoleIdAsync(roleId, cancellationToken);

        var assignedMenuIds = roleMenus.Select(rm => rm.MenuId).ToHashSet();
        var roleMenuActionIds = roleMenus
            .SelectMany(rm => rm.ImRoleMenuActions)
            .Select(rma => rma.MenuActionId)
            .ToHashSet();

        var culture = _currentUserService.CultureCode;
        var menuTrans = await _translationService.GetTranslationsMapAsync("Menu", culture, cancellationToken);
        var actionTrans = await _translationService.GetTranslationsMapAsync("BusinessAction", culture, cancellationToken);

        return BuildRoleMenuTree(allMenus.ToList(), null, assignedMenuIds, roleMenuActionIds, menuTrans, actionTrans);
    }

    public async Task<bool> AssignRoleMenusAsync(Guid roleId, AssignRoleMenusRequest request, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken);
        if (role == null || role.IsDeleted)
            throw new NotFoundException(nameof(ImRole), roleId);

        var existingRoleMenus = await _roleMenuRepository.FindAsync(rm => rm.RoleId == roleId, cancellationToken);
        var existingRoleMenuIds = existingRoleMenus.Select(rm => rm.Id).ToList();

        var existingRoleMenuActions = await _roleMenuActionRepository.FindAsync(
            rma => existingRoleMenuIds.Contains(rma.RoleMenuId),
            cancellationToken);

        await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            _roleMenuActionRepository.DeleteRange(existingRoleMenuActions);
            _roleMenuRepository.DeleteRange(existingRoleMenus);

            foreach (var assignment in request.MenuAssignments)
            {
                var menuExists = await _menuRepository.AnyAsync(m => m.Id == assignment.MenuId && !m.IsDeleted, cancellationToken);
                if (!menuExists)
                    continue;

                var roleMenu = new ImRoleMenu
                {
                    RoleId = roleId,
                    MenuId = assignment.MenuId,
                    CreatedBy = _currentUserService.UserName
                };

                await _roleMenuRepository.AddAsync(roleMenu, cancellationToken);

                if (assignment.ActionIds != null && assignment.ActionIds.Count > 0)
                {
                    var availableMenuActions = await _menuActionRepository.FindAsync(
                        ma => ma.MenuId == assignment.MenuId && assignment.ActionIds.Contains(ma.ActionId) && ma.IsActive,
                        cancellationToken);

                    foreach (var menuAction in availableMenuActions)
                    {
                        await _roleMenuActionRepository.AddAsync(new ImRoleMenuAction
                        {
                            RoleMenuId = roleMenu.Id,
                            MenuActionId = menuAction.Id,
                            CreatedBy = _currentUserService.UserName
                        }, cancellationToken);
                    }
                }
            }
        }, cancellationToken);

        await _auditService.LogAsync(
            _currentUserService.UserId?.ToString(),
            AuditAction.Update,
            nameof(ImRoleMenu),
            roleId.ToString(),
            null,
            new { RoleName = role.Name, MenuCount = request.MenuAssignments.Count },
            _currentUserService.IpAddress,
            _currentUserService.UserAgent,
            cancellationToken);

        return true;
    }

    #region Helper Tree Builders

    private MenuResponse MapToResponse(
        ImMenu menu,
        Dictionary<Guid, (string Name, string? Description)> menuTrans,
        Dictionary<Guid, (string Name, string? Description)> actionTrans)
    {
        var response = _mapper.Map<MenuResponse>(menu);
        if (menuTrans.TryGetValue(menu.Id, out var mt) && !string.IsNullOrWhiteSpace(mt.Name))
        {
            response.Title = mt.Name;
        }

        if (response.Actions != null)
        {
            foreach (var act in response.Actions)
            {
                if (actionTrans.TryGetValue(act.Id, out var at))
                {
                    if (!string.IsNullOrWhiteSpace(at.Name)) act.Name = at.Name;
                    if (at.Description != null) act.Description = at.Description;
                }
            }
        }

        return response;
    }

    private static List<MenuTreeNodeResponse> BuildMenuTree(
        List<ImMenu> allMenus,
        Guid? parentId,
        Dictionary<Guid, (string Name, string? Description)> menuTrans,
        Dictionary<Guid, (string Name, string? Description)> actionTrans)
    {
        return allMenus
            .Where(m => m.ParentId == parentId)
            .OrderBy(m => m.OrderIndex)
            .Select(m =>
            {
                var title = menuTrans.TryGetValue(m.Id, out var mt) && !string.IsNullOrWhiteSpace(mt.Name)
                    ? mt.Name
                    : m.Title;

                var actions = m.ImMenuActions
                    .Where(ma => ma.IsActive && ma.Action != null && !ma.Action.IsDeleted)
                    .Select(ma =>
                    {
                        var actName = actionTrans.TryGetValue(ma.Action.Id, out var at) && !string.IsNullOrWhiteSpace(at.Name)
                            ? at.Name
                            : ma.Action.Name;
                        var actDesc = actionTrans.TryGetValue(ma.Action.Id, out var atDesc) && atDesc.Description != null
                            ? atDesc.Description
                            : ma.Action.Description;

                        return new BusinessActionResponse
                        {
                            Id = ma.Action.Id,
                            Name = actName,
                            Code = ma.Action.Code,
                            Description = actDesc,
                            IsActive = ma.Action.IsActive
                        };
                    })
                    .ToList();

                var children = BuildMenuTree(allMenus, m.Id, menuTrans, actionTrans);

                return new MenuTreeNodeResponse
                {
                    Id = m.Id,
                    Title = title,
                    Route = m.Route,
                    Icon = m.Icon,
                    ParentId = m.ParentId,
                    OrderIndex = m.OrderIndex,
                    IsActive = m.IsActive,
                    Actions = actions,
                    Children = children
                };
            })
            .ToList();
    }

    private static List<UserNavMenuResponse> BuildUserNavTree(
        List<ImMenu> menus,
        Guid? parentId,
        Dictionary<Guid, HashSet<string>>? menuActionMap,
        Dictionary<Guid, (string Name, string? Description)> menuTrans)
    {
        return menus
            .Where(m => m.ParentId == parentId)
            .OrderBy(m => m.OrderIndex)
            .Select(m =>
            {
                var title = menuTrans.TryGetValue(m.Id, out var mt) && !string.IsNullOrWhiteSpace(mt.Name)
                    ? mt.Name
                    : m.Title;

                var actions = menuActionMap != null && menuActionMap.TryGetValue(m.Id, out var actionSet)
                    ? actionSet.ToList()
                    : m.ImMenuActions.Where(ma => ma.IsActive && ma.Action != null).Select(ma => ma.Action.Code).ToList();

                var children = BuildUserNavTree(menus, m.Id, menuActionMap, menuTrans);

                return new UserNavMenuResponse
                {
                    Id = m.Id,
                    Title = title,
                    Route = m.Route,
                    Icon = m.Icon,
                    OrderIndex = m.OrderIndex,
                    AuthorizedActions = actions,
                    Children = children
                };
            })
            .ToList();
    }

    private static List<RoleMenuDetailsResponse> BuildRoleMenuTree(
        List<ImMenu> allMenus,
        Guid? parentId,
        HashSet<Guid> assignedMenuIds,
        HashSet<Guid> roleMenuActionIds,
        Dictionary<Guid, (string Name, string? Description)> menuTrans,
        Dictionary<Guid, (string Name, string? Description)> actionTrans)
    {
        return allMenus
            .Where(m => m.ParentId == parentId)
            .OrderBy(m => m.OrderIndex)
            .Select(m =>
            {
                var title = menuTrans.TryGetValue(m.Id, out var mt) && !string.IsNullOrWhiteSpace(mt.Name)
                    ? mt.Name
                    : m.Title;

                var actionAssignments = m.ImMenuActions
                    .Where(ma => ma.IsActive && ma.Action != null && !ma.Action.IsDeleted)
                    .Select(ma =>
                    {
                        var actName = actionTrans.TryGetValue(ma.Action.Id, out var at) && !string.IsNullOrWhiteSpace(at.Name)
                            ? at.Name
                            : ma.Action.Name;

                        return new RoleMenuActionAssignmentResponse
                        {
                            ActionId = ma.Action.Id,
                            ActionName = actName,
                            ActionCode = ma.Action.Code,
                            IsGranted = roleMenuActionIds.Contains(ma.Id)
                        };
                    })
                    .ToList();

                var children = BuildRoleMenuTree(allMenus, m.Id, assignedMenuIds, roleMenuActionIds, menuTrans, actionTrans);
                var isAssigned = assignedMenuIds.Contains(m.Id);

                return new RoleMenuDetailsResponse
                {
                    MenuId = m.Id,
                    Title = title,
                    Route = m.Route,
                    Icon = m.Icon,
                    ParentId = m.ParentId,
                    OrderIndex = m.OrderIndex,
                    IsAssigned = isAssigned,
                    Actions = actionAssignments,
                    Children = children
                };
            })
            .ToList();
    }

    #endregion
}
