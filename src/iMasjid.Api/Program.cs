using iMasjid.Api.Extensions;
using iMasjid.Application;
using iMasjid.Infrastructure;
using iMasjid.Infrastructure.Persistence.Seed;
using iMasjid.Jobs;

var builder = WebApplication.CreateBuilder(args);

// Add Clean Architecture Layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddBackgroundJobs(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

// Seed Database (Roles, Permissions, Default SuperAdmin)
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

// Configure Middleware Pipeline
app.UseApiMiddlewarePipeline(app.Environment);

app.Run();

// Required for Integration Testing with WebApplicationFactory<Program>
public partial class Program { }
