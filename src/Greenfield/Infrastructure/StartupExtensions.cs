using Greenfield.Infrastructure.Behaviors;
using MediatR;

namespace Greenfield.Infrastructure;

public static class StartupExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(options =>
        {
            options.RegisterServicesFromAssemblyContaining<Program>();

            options.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        });

        return services;
    }
}