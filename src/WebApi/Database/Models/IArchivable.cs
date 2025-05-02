namespace WebApi.Database.Models;

/// <summary>
///     Represents a marker interface for soft-deletable entities.
/// </summary>
internal interface IArchivable
{
    /// <summary>
    ///     Gets the <see cref="DateTime" /> the entity was soft-deleted on.
    /// </summary>
    DateTime? ArchivedOnUtc { get; set; }
}