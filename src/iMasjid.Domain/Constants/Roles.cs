namespace iMasjid.Domain.Constants;

public static class Roles
{
    public const string SuperAdmin = "Super Admin";
    public const string MinistryAdmin = "Ministry Admin";
    public const string SuperMosqueAdmin = "Super Mosque Admin";
    public const string MosqueAdmin = "Mosque Admin";
    public const string MosqueAssistant = "Mosque Assistant";
    public const string Imam = "Imam";
    public const string Member = "Member";
    public const string OfficeBearer = "Office Bearer";
    public const string SupportTeam = "Support Team";
    public const string Developer = "Developer";
    public const string Vendor = "Vendor";
    public const string Technician = "Technician";
    public const string Accountant = "Accountant";

    // Compatibility aliases
    public const string Admin = MosqueAdmin;
    public const string User = Member;

    public static readonly IReadOnlyList<string> All = new[]
    {
        SuperAdmin,
        MinistryAdmin,
        SuperMosqueAdmin,
        MosqueAdmin,
        MosqueAssistant,
        Imam,
        Member,
        OfficeBearer,
        SupportTeam,
        Developer,
        Vendor,
        Technician,
        Accountant
    };
}
