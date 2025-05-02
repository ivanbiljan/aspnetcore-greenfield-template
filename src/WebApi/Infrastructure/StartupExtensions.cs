namespace WebApi.Infrastructure;

internal static class StartupExtensions
{
    public static IServiceCollection AddWebApiServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        
        services.AutoRegisterFromWebApi();
        services.AutoConfigureOptions();

        services.AddSingleton(_ => TimeProvider.System);
        
        return services;
    }
}