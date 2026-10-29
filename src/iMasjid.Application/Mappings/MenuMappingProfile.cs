using AutoMapper;
using iMasjid.Application.DTOs.Responses.Actions;
using iMasjid.Application.DTOs.Responses.Menus;
using iMasjid.Domain.Entities;

namespace iMasjid.Application.Mappings;

public sealed class MenuMappingProfile : Profile
{
    public MenuMappingProfile()
    {
        CreateMap<ImBusinessAction, BusinessActionResponse>();

        CreateMap<ImMenu, MenuResponse>()
            .ForMember(d => d.Actions,
                       o => o.MapFrom(s => s.ImMenuActions
                           .Where(ma => ma.IsActive && ma.Action != null && !ma.Action.IsDeleted)
                           .Select(ma => ma.Action)
                           .ToList()));
    }
}
