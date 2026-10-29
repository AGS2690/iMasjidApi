using iMasjid.Domain.Common;

namespace iMasjid.Domain.Entities;

public class ImLocale : AuditableEntity<Guid>
{
    public int DisplayOrder { get; set; } = 1;
    public string CountryName { get; set; } = string.Empty;
    public string PhoneCode { get; set; } = string.Empty;
    public string LanguageName { get; set; } = string.Empty;
    public string CultureCode { get; set; } = string.Empty;
    public string CurrencySymbol { get; set; } = string.Empty;
    public string CurrencyName { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public string Direction { get; set; } = "LTR";
    public string FlagIconUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
