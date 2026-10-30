using System.Reflection;
using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using iMasjid.Application.Features.Actions;
using iMasjid.Application.Features.AuditLogs;
using iMasjid.Application.Features.Auth;
using iMasjid.Application.Features.Menus;
using iMasjid.Application.Features.Roles;
using iMasjid.Application.Features.Settings;
using iMasjid.Application.Features.Users;
using iMasjid.Application.Interfaces.Features;
using iMasjid.Application.Mappings;

namespace iMasjid.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<UserMappingProfile>();
            cfg.AddProfile<RoleMappingProfile>();
            cfg.AddProfile<MenuMappingProfile>();
            cfg.AddProfile<SettingMappingProfile>();
            cfg.AddProfile<AuditLogMappingProfile>();
            cfg.AddProfile<LocaleMappingProfile>();
        });

        services.AddValidatorsFromAssembly(assembly);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IApplicationSettingService, ApplicationSettingService>();
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<IBusinessActionService, BusinessActionService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<ILocaleService, iMasjid.Application.Features.Localization.LocaleService>();

        return services;
    }
}
