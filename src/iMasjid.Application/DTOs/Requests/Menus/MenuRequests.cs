namespace iMasjid.Application.DTOs.Requests.Menus;

public abstract class MenuBaseDto
{
    public string Title { get; set; } = string.Empty;

    public string? Route { get; set; }

    public string? Icon { get; set; }

    public Guid? ParentId { get; set; }

    public int OrderIndex { get; set; }

    public bool IsActive { get; set; } = true;
}

public class CreateMenuRequest : MenuBaseDto
{
    public IReadOnlyList<Guid>? ActionIds { get; set; }
}

public class UpdateMenuRequest : MenuBaseDto
{
}

public class AssignMenuActionsRequest
{
    public IReadOnlyList<Guid> ActionIds { get; set; } = [];
}

public class RoleMenuAssignmentItem
{
    public Guid MenuId { get; set; }

    public IReadOnlyList<Guid> ActionIds { get; set; } = [];
}

public class AssignRoleMenusRequest
{
    public IReadOnlyList<RoleMenuAssignmentItem> MenuAssignments { get; set; } = [];
}
