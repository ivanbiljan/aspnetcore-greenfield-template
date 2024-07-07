using MediatR;
using Microsoft.AspNetCore.Identity;
using SourceGenerators;
using WebApi.Infrastructure.Behaviors;
using WebApi.Infrastructure.Persistence;

namespace WebApi.Infrastructure;

public static class StartupExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
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
        
        services.AddMediatR(
            options =>
            {
                options.RegisterServicesFromAssemblyContaining<Program>();
                
                options.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
                options.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            }
        );
        
        return services;
    }
}