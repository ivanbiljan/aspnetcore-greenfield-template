namespace Api.Infrastructure.Hangfire.Filters;

[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class FingerprintIgnoreAttribute : Attribute
{
}