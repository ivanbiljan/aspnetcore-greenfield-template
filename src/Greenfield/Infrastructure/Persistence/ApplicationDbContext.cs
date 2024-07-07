using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Greenfield.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    /// <inheritdoc />
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        
        configurationBuilder.Conventions.Remove<TableNameFromDbSetConvention>();
    }
    
    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<IdentityUser>().ToTable("aspnet_web_user");
        builder.Entity<IdentityRole>().ToTable("aspnet_web_role");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("aspnet_web_role_claim");
        builder.Entity<IdentityUserClaim<string>>().ToTable("aspnet_web_user_claim");
        builder.Entity<IdentityUserLogin<string>>().ToTable("aspnet_web_user_login");
        builder.Entity<IdentityUserRole<string>>().ToTable("aspnet_web_user_role");
        builder.Entity<IdentityUserToken<string>>().ToTable("aspnet_web_user_token");
    }
}