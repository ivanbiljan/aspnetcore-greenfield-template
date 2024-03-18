namespace Greenfield.Infrastructure.Persistence.Models;

/// <summary>
///     Represents a marker interface for soft-deletable entities.
/// </summary>
public interface IArchivable
{
    /// <summary>
    ///     Gets the <see cref="DateTime" /> the entity was soft-deleted on.
    /// </summary>
    DateTime? ArchivedOnUtc { get; set; }
}