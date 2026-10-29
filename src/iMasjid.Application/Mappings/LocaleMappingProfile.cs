using AutoMapper;
using iMasjid.Application.DTOs.Responses.Localization;
using iMasjid.Domain.Entities;

namespace iMasjid.Application.Mappings;

public sealed class LocaleMappingProfile : Profile
{
    public LocaleMappingProfile()
    {
        CreateMap<ImLocale, SupportedLanguageResponse>();
    }
}
