namespace iMasjid.Application.DTOs.Requests.Menus;

public abstract class MenuBaseDto
{
    /// <example>Dashboard</example>
    public string Title { get; set; } = string.Empty;

    /// <example>/dashboard</example>
    public string? Route { get; set; }

    /// <example>dashboard</example>
    public string? Icon { get; set; }

    /// <example>null</example>
    public Guid? ParentId { get; set; }

    /// <example>1</example>
    public int OrderIndex { get; set; }

    /// <example>true</example>
    public bool IsActive { get; set; } = true;
}

public class CreateMenuRequest : MenuBaseDto
{
    /// <example>["3fa85f64-5717-4562-b3fc-2c963f66afa6"]</example>
    public IReadOnlyList<Guid>? ActionIds { get; set; }
}

public class UpdateMenuRequest : MenuBaseDto
{
}

public class AssignMenuActionsRequest
{
    /// <example>["3fa85f64-5717-4562-b3fc-2c963f66afa6", "7c9e6679-7425-40de-944b-e07fc1f90ae7"]</example>
    public IReadOnlyList<Guid> ActionIds { get; set; } = [];
}

public class RoleMenuAssignmentItem
{
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid MenuId { get; set; }

    /// <example>["7c9e6679-7425-40de-944b-e07fc1f90ae7"]</example>
    public IReadOnlyList<Guid> ActionIds { get; set; } = [];
}

public class AssignRoleMenusRequest
{
    public IReadOnlyList<RoleMenuAssignmentItem> MenuAssignments { get; set; } = [];
}
