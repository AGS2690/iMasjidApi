using iMasjid.Application.DTOs.Requests.Actions;

namespace iMasjid.Application.DTOs.Responses.Actions;

public class BusinessActionResponse : BusinessActionBaseDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
}
