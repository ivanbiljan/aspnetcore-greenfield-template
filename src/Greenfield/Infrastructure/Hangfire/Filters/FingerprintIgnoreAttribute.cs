namespace Greenfield.Infrastructure.Hangfire.Filters;

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class FingerprintIgnoreAttribute : Attribute
{
}