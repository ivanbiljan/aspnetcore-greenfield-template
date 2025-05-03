using Immediate.Handlers.Shared;
using WebApi.Database;

namespace WebApi.Tests.Infrastructure;

[Collection(IntegrationTestCollectionContainer.FixtureName)]
public abstract class IntegrationTestBase(CustomApplicationFactory factory) : IAsyncLifetime
{
    protected CustomApplicationFactory Factory { get; } = factory;

    protected ApplicationDbContext DbContext => GetService<ApplicationDbContext>();

    public async Task<TResponse> Send<TRequest, TResponse>(TRequest request)
    {
        var sender = GetService<IHandler<TRequest, TResponse>>();

        return await sender.HandleAsync(request);
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

    public async Task DeleteAsync<TEntity>(params TEntity[] entities) where TEntity : class
    {
        DbContext.Set<TEntity>().RemoveRange(entities);
        await DbContext.SaveChangesAsync();
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await Factory.ResetDatabaseAsync();
    }
}