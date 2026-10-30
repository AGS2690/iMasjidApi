using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using iMasjid.Api.Middleware;
using iMasjid.Jobs;

using Swashbuckle.AspNetCore.SwaggerUI;

namespace iMasjid.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseApiMiddlewarePipeline(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseForwardedHeaders();
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseMiddleware<CorrelationIdMiddleware>();       
        app.UseMiddleware<RequestLoggingMiddleware>();
        if (!env.IsDevelopment())
        {
            app.UseHsts();
        }
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "iMasjid API");
                c.RoutePrefix = "swagger"; 
                c.DocExpansion(DocExpansion.None);
                c.DefaultModelsExpandDepth(-1); 
                c.DisplayRequestDuration(); 
            });
        }
        
        app.UseCors(ApiConstants.CorsPolicyName);
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseBackgroundJobsDashboard();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var response = new
                    {
                        status = report.Status.ToString()
                    };
                    await context.Response.WriteAsJsonAsync(response);
                }
            });
        });

        return app;
    }
}
