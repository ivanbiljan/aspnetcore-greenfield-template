namespace Greenfield.Tests.Infrastructure;

public static class Extensions
{
    public static IServiceCollection Remove<TService>(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        
        var serviceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(TService));
        if (serviceDescriptor is not null)
        {
            services.Remove(serviceDescriptor);
        }
        
        return services;
    }
}