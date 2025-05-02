using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WebApi.Infrastructure.Database.Interceptors;

namespace WebApi.Infrastructure.Database;

internal static class StartupExtensions
{
    public static IHostApplicationBuilder AddEntityFramework(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        
        var connectionString = builder.Configuration.GetConnectionString("Npgsql");

        builder.Services.AddSingleton<SlowQueryLoggingInterceptor>();
        builder.Services.AddSingleton<AuditLoggingInterceptor>();
        builder.Services.AddSingleton<IArchivableInterceptor>();
        
        builder.Services.AddDbContext<ApplicationDbContext>(
            (provider, options) =>
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

                options.AddInterceptors(
                    provider.GetRequiredService<SlowQueryLoggingInterceptor>(),
                    provider.GetRequiredService<AuditLoggingInterceptor>(),
                    provider.GetRequiredService<IArchivableInterceptor>()
                );
            }
        );
        
        return builder;
    }
}