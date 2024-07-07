using MediatR;

namespace Greenfield.Tests.Extensions;

[Collection(IntegrationTestCollectionContainer.FixtureName)]
public abstract class IntegrationTestBase(CustomApplicationFactory factory)
{
    protected CustomApplicationFactory Factory { get; } = factory;
    
    protected ApplicationDbContext DbContext => GetService<ApplicationDbContext>();
    
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request)
    {
        var sender = GetService<ISender>();
        
        return await sender.Send(request);
    }
    
    public async Task Send(IRequest request)
    {
        var sender = GetService<ISender>();
        await sender.Send(request);
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
}