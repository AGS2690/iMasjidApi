using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using iMasjid.Api.Extensions;
using iMasjid.Application;
using iMasjid.Infrastructure;
using iMasjid.Jobs;
using iMasjid.Application.Interfaces.Services;

namespace iMasjid.SmokeTests;

public class TestFixture : IDisposable
{
    public IServiceProvider ServiceProvider { get; }
    private readonly ConcurrentDictionary<string, string> _otpCodes = new();

    public TestFixture()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Testing",
            ["ConnectionStrings:Redis"] = "",
            ["Security:OtpHashKey"] = "test-only-otp-hmac-key-change-me-32-bytes",
            ["Email:SmtpServer"] = "smtp.example.com",
            ["Email:Port"] = "587",
            ["Email:SenderEmail"] = "noreply@imasjid.com",
            ["Email:SenderName"] = "iMasjid Security",
            ["Email:Enabled"] = "false",
            ["Cors:AllowedOrigins:0"] = "http://localhost:5174"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();

        services.AddLogging(logging => logging.AddConsole());
        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddBackgroundJobs(configuration);
        services.AddScoped<IEmailService>(_ => new CapturingEmailService(_otpCodes));
        services.AddApiServices(configuration);

        ServiceProvider = services.BuildServiceProvider();
    }

    public IServiceScope CreateScope() => ServiceProvider.CreateScope();

    public string GetOtp(string email, string purpose)
        => _otpCodes.TryGetValue($"{email}|{purpose}", out var otp)
            ? otp
            : throw new InvalidOperationException($"No captured OTP exists for {email} and purpose {purpose}.");

    public void Dispose()
    {
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
