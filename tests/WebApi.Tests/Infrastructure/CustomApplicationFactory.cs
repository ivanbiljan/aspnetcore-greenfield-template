﻿using WebApi.Infrastructure;
using WebApi.Infrastructure.Logging;
using WebApi.Infrastructure.Web;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Moq;
using Testcontainers.PostgreSql;

namespace WebApi.Tests.Infrastructure;

public sealed class CustomApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage(PostgreSqlBuilder.PostgreSqlImage)
        .WithDatabase(PostgreSqlBuilder.DefaultDatabase)
        .WithUsername(PostgreSqlBuilder.DefaultUsername)
        .WithPassword(PostgreSqlBuilder.DefaultPassword)
        .Build();
    
    public IServiceScope ServiceScope { get; private set; } = null!;
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(
            configuration =>
            {
                configuration.Sources.Clear();
                configuration.SetBasePath(Directory.GetCurrentDirectory());
                configuration.AddJsonFile("appsettings.json");
                configuration.Build();
            }
        );
        
        builder.ConfigureTestServices(
            services =>
            {
                var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
                httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());
                
                services.Remove<IHttpContextAccessor>()
                    .AddSingleton(httpContextAccessorMock.Object);
                
                services
                    .Remove<DbContextOptions<ApplicationDbContext>>()
                    .AddDbContext<ApplicationDbContext>(
                        options =>
                            options.UseNpgsql(_postgreSqlContainer.GetConnectionString())
                    );
                
                services.AddSerilogInternal("WebApi");
                services.ConfigureSerilogHttpLogging();
                services.AddApplicationServices();
                
                var currentUserServiceMock = new Mock<ICurrentUserService>();
                currentUserServiceMock.Setup(s => s.UserId).Returns("AdminId");
                services.Remove<ICurrentUserService>()
                    .AddScoped<ICurrentUserService>(_ => currentUserServiceMock.Object);
            }
        );
    }
    
    public async Task InitializeAsync()
    {
        ServiceScope = Services.CreateScope();
        await _postgreSqlContainer.StartAsync();
        
        var dbContext = ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }
    
    public new async Task DisposeAsync()
    {
        ServiceScope.Dispose();
        await _postgreSqlContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}