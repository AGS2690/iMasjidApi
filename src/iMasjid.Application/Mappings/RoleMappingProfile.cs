using AutoMapper;
using iMasjid.Application.DTOs.Responses.Roles;
using iMasjid.Domain.Entities;

namespace iMasjid.Application.Mappings;

public sealed class RoleMappingProfile : Profile
{
    public RoleMappingProfile()
    {
        CreateMap<ImPermission, PermissionDto>();

        CreateMap<ImRole, RoleResponse>()
            .ForMember(d => d.Permissions,
                       o => o.MapFrom((src, _, _, ctx) =>
                           ctx.Items != null && ctx.Items.TryGetValue("Permissions", out var p) && p is IReadOnlyList<string> customPerms
                               ? customPerms
                               : (src.ImRolePermissions != null
                                   ? src.ImRolePermissions
                                       .Where(rp => rp != null && rp.Permission != null && rp.Permission.IsActive && !rp.Permission.IsDeleted)
                                       .Select(rp => rp.Permission.Code)
                                       .ToList()
                                   : new List<string>())));
    }
}
