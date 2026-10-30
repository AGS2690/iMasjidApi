namespace iMasjid.Application.DTOs.Requests.Settings;

public abstract class SettingBaseDto
{
    public string Value { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public class CreateSettingRequest : SettingBaseDto
{
    public string Key { get; set; } = string.Empty;

    public string Module { get; set; } = string.Empty;

    public bool IsEncrypted { get; set; }
}

public class UpdateSettingRequest : SettingBaseDto
{
}
