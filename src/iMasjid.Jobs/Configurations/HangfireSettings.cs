namespace iMasjid.Jobs.Configurations;

public class HangfireSettings
{
    public const string SectionName = "Hangfire";

    public string DashboardPath { get; set; } = "/hangfire";
    public string DashboardUsername { get; set; } = "admin";
    public string DashboardPassword { get; set; } = "SuperAdmin@123";
    public int WorkerCount { get; set; } = 5;
}
