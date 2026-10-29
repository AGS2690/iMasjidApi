namespace iMasjid.Application.DTOs.Requests.Settings;

public abstract class SettingBaseDto
{
    /// <example>Dark</example>
    public string Value { get; set; } = string.Empty;

    /// <example>Controls the default UI theme for all users.</example>
    public string? Description { get; set; }
}

public class CreateSettingRequest : SettingBaseDto
{
    /// <example>App:Theme</example>
    public string Key { get; set; } = string.Empty;

    /// <example>UI</example>
    public string Module { get; set; } = string.Empty;

    /// <example>false</example>
    public bool IsEncrypted { get; set; }
}

public class UpdateSettingRequest : SettingBaseDto
{
}
