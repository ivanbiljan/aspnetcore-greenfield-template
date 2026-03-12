using System.Diagnostics.CodeAnalysis;
using Api.Database;
using Immediate.Handlers.Shared;

namespace Api.Tests.Infrastructure;

[CollectionDefinition(FixtureName)]
[SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Justification = "xUnit collection definition classes must be public"
)]
public sealed class IntegrationTestCollectionContainer : ICollectionFixture<CustomApplicationFactory>
{
    public const string FixtureName = "IntegrationTestFixture";
}

[Collection(IntegrationTestCollectionContainer.FixtureName)]
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix")]
public abstract class IntegrationTestCollection(CustomApplicationFactory factory) : IAsyncLifetime
{
    protected ApplicationDbContext DbContext => GetService<ApplicationDbContext>();

    protected CustomApplicationFactory Factory { get; } = factory;

    public async Task DisposeAsync()
    {
        await Factory.ResetDatabaseAsync();
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DeleteAsync<TEntity>(params TEntity[] entities) where TEntity : class
    {
        DbContext.Set<TEntity>().RemoveRange(entities);
        await DbContext.SaveChangesAsync();
    }

    public TService GetService<TService>() where TService : notnull
    {
        return Factory.ServiceScope.ServiceProvider.GetRequiredService<TService>();
    }

    public async Task InsertAsync<TEntity>(params TEntity[] entities) where TEntity : class
    {
        DbContext.Set<TEntity>().AddRange(entities);
        await DbContext.SaveChangesAsync();
    }

    public async Task<TResponse> Send<TRequest, TResponse>(TRequest request)
    {
        var sender = GetService<IHandler<TRequest, TResponse>>();

        return await sender.HandleAsync(request);
    }
}