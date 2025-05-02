using System.Diagnostics.CodeAnalysis;

namespace WebApi.Infrastructure.Database;

internal sealed class AuditContext : IDisposable
{
    private static readonly AsyncLocal<AuditContext?> ActiveContext = new();

    private readonly IDictionary<string, object?> _extraFields = new Dictionary<string, object?>();
    private readonly AuditContext? _parentContext;

    private bool _isDisposed;

    public AuditContext()
    {
        _parentContext = ActiveContext.Value;
        ActiveContext.Value = this;
    }

    public AuditContext(IDictionary<string, object?> extraFields) : this()
    {
        _extraFields = extraFields;
    }

    public AuditContext(params (string key, object? value)[] extraFields) : this()
    {
        _extraFields = extraFields.ToDictionary(x => x.key, x => x.value);
    }

    public static AuditContext? Current => ActiveContext.Value;

    public string? AuditedBy { get; set; }

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    public Dictionary<string, object?> ExtraFields
    {
        get
        {
            var properties = new Dictionary<string, object?>();
            var context = ActiveContext.Value;

            while (context is not null)
            {
                foreach (var kvp in context._extraFields)
                {
                    if (!properties.ContainsKey(kvp.Key))
                    {
                        properties[kvp.Key] = kvp.Value;
                    }
                }

                context = context._parentContext;
            }

            return properties;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _extraFields.Clear();
        ActiveContext.Value = _parentContext;
        _isDisposed = true;
    }

    public AuditContext SetProperty(string key, object? value)
    {
        lock (_extraFields)
        {
            _extraFields[key] = value;
        }

        return this;
    }
}