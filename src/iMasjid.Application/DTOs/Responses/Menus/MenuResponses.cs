using iMasjid.Application.DTOs.Requests.Menus;
using iMasjid.Application.DTOs.Responses.Actions;

namespace iMasjid.Application.DTOs.Responses.Menus;

public class MenuResponse : MenuBaseDto
{
    public Guid Id { get; set; }
    public IReadOnlyList<BusinessActionResponse> Actions { get; set; } = [];
}

public class MenuTreeNodeResponse : MenuResponse
{
    public IReadOnlyList<MenuTreeNodeResponse> Children { get; set; } = [];
}

public class UserNavMenuResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Route { get; set; }
    public string? Icon { get; set; }
    public int OrderIndex { get; set; }
    public IReadOnlyList<string> AuthorizedActions { get; set; } = [];
    public IReadOnlyList<UserNavMenuResponse> Children { get; set; } = [];
}

public class RoleMenuActionAssignmentResponse
{
    public Guid ActionId { get; set; }
    public string ActionName { get; set; } = string.Empty;
    public string ActionCode { get; set; } = string.Empty;
    public bool IsGranted { get; set; }
}

public class RoleMenuDetailsResponse
{
    public Guid MenuId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Route { get; set; }
    public string? Icon { get; set; }
    public Guid? ParentId { get; set; }
    public int OrderIndex { get; set; }
    public bool IsAssigned { get; set; }
    public IReadOnlyList<RoleMenuActionAssignmentResponse> Actions { get; set; } = [];
    public IReadOnlyList<RoleMenuDetailsResponse> Children { get; set; } = [];
}
