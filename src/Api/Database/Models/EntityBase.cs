namespace Api.Database.Models;

public abstract class EntityBase<TKey>
{
    public DateTime CreatedOnUtc { get; init; } = DateTime.UtcNow;

    public TKey Id { get; init; } = default!;
}