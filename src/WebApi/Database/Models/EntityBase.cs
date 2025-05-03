namespace WebApi.Database.Models;

public abstract class EntityBase<TKey>
{
    public TKey Id { get; init; } = default!;

    public DateTime CreatedOnUtc { get; init; } = DateTime.UtcNow;
}