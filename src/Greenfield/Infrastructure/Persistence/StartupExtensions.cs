using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Greenfield.Infrastructure.Persistence;

public static class StartupExtensions
{
    public static IHostApplicationBuilder AddEntityFramework(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        
        var connectionString = builder.Configuration.GetConnectionString("Npgsql");
        
        builder.Services.AddDbContext<ApplicationDbContext>(
            options =>
            {
                options.EnableDetailedErrors();
                if (builder.Environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                    options.ConfigureWarnings(
                        warningsConfiguration =>
                        {
                            warningsConfiguration.Log(
                                CoreEventId.FirstWithoutOrderByAndFilterWarning,
                                CoreEventId.RowLimitingOperationWithoutOrderByWarning,
                                CoreEventId.StartedTracking,
                                CoreEventId.SaveChangesStarting
                            );
                        }
                    );
                }
                
                options.UseNpgsql(
                        connectionString,
                        configuration =>
                        {
                            configuration.EnableRetryOnFailure(3);
                        }
                    )
                    .UseSnakeCaseNamingConvention();
            }
        );
        
        return builder;
    }
}