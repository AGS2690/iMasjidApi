namespace iMasjid.Domain.Entities;

public class VwUserEffectivePermission
{
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? PermissionCode { get; set; }
    public string? PermissionName { get; set; }
    public string? PermissionModule { get; set; }
    public string? Source { get; set; }
}
