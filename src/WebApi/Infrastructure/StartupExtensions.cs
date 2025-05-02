using Microsoft.AspNetCore.Identity;

namespace WebApi.Infrastructure;

internal static class StartupExtensions
{
    public static IServiceCollection AddWebApiServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        
        services.AutoRegister();
        services.AutoConfigureOptions();
        
        services.AddDataProtection();
        services.AddIdentityCore<IdentityUser>(
                options =>
                {
                    options.User.RequireUniqueEmail = true;
                    options.SignIn.RequireConfirmedAccount = true;
                }
            )
            .AddRoles<IdentityRole>()
            .AddDefaultTokenProviders()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddApiEndpoints();

        services.AddSingleton(_ => TimeProvider.System);
        
        return services;
    }
}