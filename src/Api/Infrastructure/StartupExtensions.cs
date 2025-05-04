using Api.Infrastructure.Localization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Api.Infrastructure;

internal static class StartupExtensions
{
    public static IServiceCollection AddWebApiServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AutoRegisterFromApi();
        services.AutoConfigureOptions();

        services.AddSingleton(_ => TimeProvider.System);

        services.AddScoped(typeof(IPasswordHasher<>), typeof(PasswordHasher<>));
        services
            .TryAddSingleton<IStringLocalizerWithCultureFactory, ResourceManagerStringLocalizerWithCultureFactory>();

        services.AddHttpContextAccessor();

        services.AddApiBehaviors();
        services.AddApiHandlers();

        return services;
    }
}