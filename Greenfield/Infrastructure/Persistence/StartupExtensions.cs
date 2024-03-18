using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Greenfield.Infrastructure.Persistence;

public static class StartupExtensions
{
    public static IHostApplicationBuilder ConfigureEntityFramework(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDbContext<DatabaseContext>(
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
                                CoreEventId.SaveChangesStarting);
                        });
                }

                var connectionString = builder.Configuration.GetConnectionString("Npgsql");

                options.UseNpgsql(connectionString, configuration => { configuration.EnableRetryOnFailure(3); });
            });

        return builder;
    }
}