using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace WebApi.Database.Interceptors;

internal sealed class AuditLoggingInterceptor(IHttpContextAccessor httpContextAccessor) : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        CaptureAuditLogEntries(eventData);

        return result;
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        CaptureAuditLogEntries(eventData);

        return new ValueTask<InterceptionResult<int>>(result);
    }

    private void CaptureAuditLogEntries(DbContextEventData eventData)
    {
        if (eventData.Context is null)
        {
            return;
        }

        foreach (var entry in eventData.Context.ChangeTracker.Entries())
        {
            if (entry.Metadata.FindAnnotation(Annotations.LogAuditTrail) is not {Value: true})
            {
                continue;
            }
            
            if (entry.State is not EntityState.Added and not EntityState.Modified and not EntityState.Deleted)
            {
                continue;
            }

            var modifiedProperties = entry.Properties
                .Where(p => p.Metadata.FindAnnotation(Annotations.ExcludeFromAuditTrail) is {Value: false})
                .Where(p => p.IsModified)
                .Select(p => p.Metadata.Name)
                .ToArray();

            if (modifiedProperties.Length == 0)
            {
                continue;
            }

            var auditContext = AuditContext.Current;

            var primaryKey = entry.Properties
                .Where(p => p.Metadata.IsPrimaryKey())
                .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);

            var httpContext = _httpContextAccessor.HttpContext;
            var (traceId, spanId) = Activity.Current is { } activity
                ? (activity.TraceId, activity.SpanId)
                : (default(ActivityTraceId), default(ActivitySpanId));

            var auditEntry = new AuditLog
            {
                TableName = entry.Entity.GetType().Name,
                Type = entry.State.ToString(),
                OldValues = SerializeProperties(entry.OriginalValues, modifiedProperties),
                NewValues = SerializeProperties(entry.CurrentValues, modifiedProperties),
                AffectedColumns = modifiedProperties,
                PrimaryKey = primaryKey.Count != 0
                    ? JsonSerializer.SerializeToDocument(primaryKey)
                    : JsonDocument.Parse("{}"),
                Actor = auditContext?.AuditedBy,
                RequestPath = httpContext is not null
                    ? $"{httpContext.Request.Method} {httpContext.Request.GetEncodedPathAndQuery()}"
                    : null,
                TraceId = traceId.ToString(),
                SpanId = spanId.ToString(),
                ExtraProperties = JsonSerializer.SerializeToDocument(auditContext?.ExtraFields ?? [])
            };

            eventData.Context.Add(auditEntry);
        }
    }

    private static JsonDocument SerializeProperties(PropertyValues propertyValues, IEnumerable<string> propertyNames)
    {
        var selectedProperties = propertyValues.Properties
            .Where(p => propertyNames.Contains(p.Name))
            .ToDictionary(
                p => p.Name,
                p =>
                {
                    var value = propertyValues[p];
                    if (value is null)
                    {
                        return value;
                    }

                    var type = value.GetType();

                    return type.IsEnum ? Enum.GetName(type, value) : value;
                }
            );

        return JsonSerializer.SerializeToDocument(selectedProperties);
    }
}