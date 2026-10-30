using iMasjid.Api.Extensions;
using iMasjid.Application;
using iMasjid.Infrastructure;
using iMasjid.Infrastructure.Persistence.Seed;
using iMasjid.Jobs;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("Secrets.Local.Json", optional: true, reloadOnChange: false);
}

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddBackgroundJobs(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    try
    {
        await AppDbInitializer.SeedDatabaseAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database on startup.");
    }
}

app.UseApiMiddlewarePipeline(app.Environment);

app.Run();

public partial class Program { }
