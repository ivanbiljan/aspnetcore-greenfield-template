﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WebApi.Infrastructure.Persistence.Models;

namespace WebApi.Infrastructure.Persistence.Interceptors;

/// <summary>
///     Represents an interceptor that provides soft-delete functionality to <see cref="IArchivable" /> entities.
/// </summary>
// ReSharper disable once InconsistentNaming
internal sealed class IArchivableInterceptor : SaveChangesInterceptor
{
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
            archivableEntity.ArchivedOnUtc = DateTime.UtcNow;
        }
        
        return ValueTask.FromResult(result);
    }
}