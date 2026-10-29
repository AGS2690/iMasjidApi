using AutoMapper;
using iMasjid.Application.DTOs.Responses.AuditLogs;
using iMasjid.Domain.Entities;

namespace iMasjid.Application.Mappings;

public sealed class AuditLogMappingProfile : Profile
{
    public AuditLogMappingProfile()
    {
        CreateMap<ImAuditLog, AuditLogResponse>();
    }
}
