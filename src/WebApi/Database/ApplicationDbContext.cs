using System.Reflection;
using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace WebApi.Database;

internal sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<UserAuthenticationToken> UserAuthenticationTokens => Set<UserAuthenticationToken>();

    /// <inheritdoc />
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.Conventions.Remove<TableNameFromDbSetConvention>();
        configurationBuilder.Properties<Enum>().HaveConversion<string>();
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
    }

    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.UseExceptionProcessor();
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            entityType.AddAnnotation(
                Annotations.LogAuditTrail,
                entityType.ClrType.GetCustomAttribute<LogAuditTrailAttribute>(true) is not null
            );

            foreach (var property in entityType.GetProperties())
            {
                property.AddAnnotation(
                    Annotations.ExcludeFromAuditTrail,
                    entityType.ClrType.GetCustomAttribute<ExcludeFromAuditTrailAttribute>(false) is not null
                );
            }
        }
    }
}