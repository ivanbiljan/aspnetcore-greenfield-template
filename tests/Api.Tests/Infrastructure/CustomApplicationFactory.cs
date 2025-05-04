using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Moq;
using Npgsql;
using Respawn;
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

    private NpgsqlConnection _npgsqlConnection = null!;
    private Respawner _respawner = null!;

    public IServiceScope ServiceScope { get; private set; } = null!;

    public IConfiguration Configuration => Services.GetRequiredService<IConfiguration>();

    public async Task InitializeAsync()
    {
        ServiceScope = Services.CreateScope();
        await _postgreSqlContainer.StartAsync();

        var dbContext = ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        _npgsqlConnection = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
        await _npgsqlConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(
            _npgsqlConnection,
            new RespawnerOptions
            {
                TablesToIgnore = ["__EFMigrationsHistory"],
                DbAdapter = DbAdapter.Postgres
            }
        );
    }

    public new async Task DisposeAsync()
    {
        ServiceScope.Dispose();
        await _postgreSqlContainer.DisposeAsync();
        await _npgsqlConnection.DisposeAsync();
        await base.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_npgsqlConnection);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configuration =>
            {
                configuration.Sources.Clear();
                configuration.SetBasePath(Directory.GetCurrentDirectory());
                configuration.AddJsonFile("appsettings.json");
                configuration.Build();
            }
        );

        builder.ConfigureTestServices(services =>
        {
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());

            services.Remove<IHttpContextAccessor>()
                .AddSingleton(httpContextAccessorMock.Object);

            services
                .Remove<DbContextOptions<ApplicationDbContext>>()
                .AddDbContext<ApplicationDbContext>(options =>
                    options.UseNpgsql(_postgreSqlContainer.GetConnectionString())
                );

            services.AddSerilogInternal("WebApi");
            services.ConfigureSerilogHttpLogging();
            services.AddWebApiServices();

            var currentUserServiceMock = new Mock<ICurrentUserService>();
            currentUserServiceMock.Setup(s => s.GetId()).Returns(1);
            services.Remove<ICurrentUserService>()
                .AddScoped<ICurrentUserService>(_ => currentUserServiceMock.Object);
        }