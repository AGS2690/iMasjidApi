using AutoMapper;
using iMasjid.Application.DTOs.Responses.Users;
using iMasjid.Domain.Entities;

namespace iMasjid.Application.Mappings;

public sealed class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<ImUser, UserResponse>()
            .ForMember(d => d.FullName,
                       o => o.MapFrom(s => s.FullName))
            .ForMember(d => d.Roles,
                       o => o.MapFrom((src, _, _, ctx) =>
                           ctx.Items != null && ctx.Items.TryGetValue("Roles", out var r) && r is IReadOnlyList<string> customRoles
                               ? customRoles
                               : (src.ImUserRoles != null
                                   ? src.ImUserRoles
                                       .Where(ur => ur != null && ur.Role != null && ur.Role.IsActive && !ur.Role.IsDeleted)
                                       .Select(ur => ur.Role.Name)
                                       .ToList()
                                   : new List<string>())));
    }
}
