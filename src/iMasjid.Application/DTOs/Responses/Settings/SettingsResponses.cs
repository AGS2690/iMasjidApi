using iMasjid.Application.DTOs.Requests.Settings;

namespace iMasjid.Application.DTOs.Responses.Settings;

public class SettingResponse : SettingBaseDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public bool IsEncrypted { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? ModifiedOn { get; set; }
}
