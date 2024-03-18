using Hangfire;
using Hangfire.Console;
using Hangfire.PostgreSql;
using Newtonsoft.Json;

namespace Greenfield.Infrastructure.Hangfire;

public static class StartupExtensions
{
    public static IHostApplicationBuilder AddHangfireInternal(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHangfire(
            hangfireConfiguration =>
            {
                var serializationSettings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                };

                hangfireConfiguration.UseSerializerSettings(serializationSettings);

                hangfireConfiguration.SetDataCompatibilityLevel(CompatibilityLevel.Version_170);
                hangfireConfiguration.UseSimpleAssemblyNameTypeSerializer();
                hangfireConfiguration.UseRecommendedSerializerSettings();
                hangfireConfiguration.UseConsole();
                hangfireConfiguration.UsePostgreSqlStorage(
                    options => options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("Hangfire")),
                    new PostgreSqlStorageOptions
                    {
                        PrepareSchemaIfNecessary = true
                    });
            });

        return builder;
    }
}