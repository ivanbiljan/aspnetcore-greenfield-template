namespace Api.Database.Models;

/// <summary>
///     Represents a marker interface for soft-deletable entities.
/// </summary>
internal interface IArchivable
{
    /// <summary>
    ///     Gets the <see cref="DateTime" /> the entity was soft-deleted on.
    /// </summary>
    Instant? ArchivedOnUtc { get; set; }
}