using Microsoft.EntityFrameworkCore.Diagnostics;
using NodaTime.Extensions;

namespace WebApi.Database.Interceptors;

/// <summary>
///     Represents an interceptor that provides soft-delete functionality to <see cref="IArchivable" /> entities.
/// </summary>
// ReSharper disable once InconsistentNaming
internal sealed class IArchivableInterceptor(TimeProvider timeProvider) : SaveChangesInterceptor
{
    private readonly TimeProvider _timeProvider = timeProvider;

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = new()
    )
    {
        if (eventData.Context is null)
        {
            return ValueTask.FromResult(result);
        }

        foreach (var entry in eventData.Context.ChangeTracker.Entries())
        {
            if (entry.Entity is not IArchivable archivableEntity || entry.State != EntityState.Deleted)
            {
                continue;
            }

            entry.State = EntityState.Modified;
            archivableEntity.ArchivedOnUtc = _timeProvider.GetCurrentInstant();
        }

        return ValueTask.FromResult(result);
    }
}