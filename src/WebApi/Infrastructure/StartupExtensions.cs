using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebApi.Infrastructure.Localization;

namespace WebApi.Infrastructure;

internal static class StartupExtensions
{
    public static IServiceCollection AddWebApiServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AutoRegisterFromWebApi();
        services.AutoConfigureOptions();

        services.AddSingleton(_ => TimeProvider.System);

        services.AddScoped(typeof(IPasswordHasher<>), typeof(PasswordHasher<>));
        services
            .TryAddSingleton<IStringLocalizerWithCultureFactory, ResourceManagerStringLocalizerWithCultureFactory>();

        services.AddWebApiBehaviors();
        services.AddWebApiHandlers();

        return services;
    }
}