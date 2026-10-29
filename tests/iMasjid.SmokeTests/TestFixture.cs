using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using iMasjid.Api.Extensions;
using iMasjid.Application;
using iMasjid.Infrastructure;
using iMasjid.Jobs;

namespace iMasjid.SmokeTests;

public class TestFixture : IDisposable
{
    public IServiceProvider ServiceProvider { get; }

    public TestFixture()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Port=5432;Database=iMasjid;Username=postgres;Password=Ags@2690;",
            ["ConnectionStrings:Redis"] = "",
            ["Jwt:SecretKey"] = "8ad3d841d4848f636132e691b4ead9cb590303ee4c486c5d048ef533399401d0d1e8a863638eb777f1a6028491f267e75b12a834e2b003840ec59d86b3b53238",
            ["Jwt:Issuer"] = "https://localhost:5174/",
            ["Jwt:Audience:0"] = "http://localhost:5174/",
            ["Jwt:Audience:1"] = "https://localhost:7199",
            ["Jwt:AccessTokenExpirationMinutes"] = "15",
            ["Jwt:RefreshTokenExpirationDays"] = "7",
            ["Email:SmtpServer"] = "smtp.example.com",
            ["Email:Port"] = "587",
            ["Email:SenderEmail"] = "noreply@imasjid.com",
            ["Email:SenderName"] = "iMasjid Security",
            ["Email:Enabled"] = "false",
            ["Cors:AllowedOrigins:0"] = "http://localhost:5174"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var services = new ServiceCollection();

        services.AddLogging(logging => logging.AddConsole());
        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddBackgroundJobs(configuration);
        services.AddApiServices(configuration);

        ServiceProvider = services.BuildServiceProvider();
    }

    public IServiceScope CreateScope() => ServiceProvider.CreateScope();

    public void Dispose()
    {
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
