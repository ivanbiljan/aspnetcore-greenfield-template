using System.Collections.Immutable;

namespace Api.Database;

internal sealed class AuditContext
{
    private static readonly AsyncLocal<AuditContextScope?> CurrentScopeAsyncLocal = new();

    public static string? AuditedBy => CurrentScope?.AuditedBy;

    public static IReadOnlyDictionary<string, object?> CurrentProperties =>
        CurrentScope?.GetMergedProperties()
        ?? ImmutableDictionary<string, object?>.Empty;

    internal static AuditContextScope? CurrentScope
    {
        get => CurrentScopeAsyncLocal.Value;
        set => CurrentScopeAsyncLocal.Value = value;
    }

    public static AuditContextScope BeginScope(params (string Key, object? Value)[] properties)
    {
        var scope = new AuditContextScope(CurrentScopeAsyncLocal.Value);

        foreach (var (key, value) in properties)
        {
            scope.SetProperty(key, value);
        }

        CurrentScopeAsyncLocal.Value = scope;

        return scope;
    }

    public static AuditContextScope BeginScope(IEnumerable<KeyValuePair<string, object?>> properties)
    {
        var scope = new AuditContextScope(CurrentScopeAsyncLocal.Value);

        foreach (var kvp in properties)
        {
            scope.SetProperty(kvp.Key, kvp.Value);
        }

        CurrentScopeAsyncLocal.Value = scope;

        return scope;
    }
}

public sealed class AuditContextScope : IDisposable
{
    private readonly AuditContextScope? _parent;
    private readonly Dictionary<string, object?> _properties = new();
    private bool _disposed;

    internal AuditContextScope(AuditContextScope? parent)
    {
        _parent = parent;
    }

    public string? AuditedBy
    {
        get => _properties.TryGetValue("AuditedBy", out var value) ? value as string : _parent?.AuditedBy;
        set
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (value is null)
            {
                _properties.Remove("AuditedBy");
            }
            else
            {
                _properties["AuditedBy"] = value;
            }
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (AuditContext.CurrentScope == this)
        {
            AuditContext.CurrentScope = _parent;
        }
    }

    public bool RemoveProperty(string key)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return _properties.Remove(key);
    }

    public void SetProperty(string key, object? value)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        _properties[key] = value;
    }

    internal IReadOnlyDictionary<string, object?> GetMergedProperties()
    {
        if (_parent is null)
        {
            return _properties;
        }

        var merged = new Dictionary<string, object?>(_parent.GetMergedProperties());

        foreach (var kvp in _properties)
        {
            merged[kvp.Key] = kvp.Value;
        }

        return merged;
    }
}