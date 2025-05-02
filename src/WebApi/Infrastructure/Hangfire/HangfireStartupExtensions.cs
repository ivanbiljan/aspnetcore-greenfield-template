using System.Diagnostics.CodeAnalysis;
using Hangfire;
using Hangfire.Console;
using Hangfire.PostgreSql;
using Newtonsoft.Json;
using WebApi.Infrastructure.Hangfire.Filters;

namespace WebApi.Infrastructure.Hangfire;

internal static class HangfireStartupExtensions
{
    [SuppressMessage("Security", "CA2326:Do not use TypeNameHandling values other than None")]
    public static IHostApplicationBuilder AddHangfireInternal(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddHangfire((provider, hangfireConfiguration) =>
            {
                var serializationSettings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                };

                hangfireConfiguration.UseSerializerSettings(serializationSettings);

                hangfireConfiguration.SetDataCompatibilityLevel(CompatibilityLevel.Version_170);
                hangfireConfiguration.UseSimpleAssemblyNameTypeSerializer();
                hangfireConfiguration.UsePostgreSqlStorage(
                    options => options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("Hangfire")),
                    new PostgreSqlStorageOptions
                    {
                        PrepareSchemaIfNecessary = true
                    }
                );

                hangfireConfiguration.UseConsole();

                hangfireConfiguration.UseFilter(new EnvironmentFilter(provider.GetRequiredService<IHostEnvironment>()));
                hangfireConfiguration.UseFilter(new AutomaticRetryAttribute {Attempts = 1});
                hangfireConfiguration.UseFilter(new HangfireJobIdEnricher());
                hangfireConfiguration.UseFilter(new AuditLogJobContextEnricher());
            }
        );

        return builder;
    }
}