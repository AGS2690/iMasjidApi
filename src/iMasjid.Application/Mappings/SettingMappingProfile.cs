using AutoMapper;
using iMasjid.Application.DTOs.Responses.Settings;
using iMasjid.Domain.Entities;

namespace iMasjid.Application.Mappings;

public sealed class SettingMappingProfile : Profile
{
    public SettingMappingProfile()
    {
        CreateMap<ImApplicationSetting, SettingResponse>()
            .ForMember(d => d.Value,
                       o => o.MapFrom(s => s.IsEncrypted ? "********" : s.Value));
    }
}
