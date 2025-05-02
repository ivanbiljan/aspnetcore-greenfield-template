namespace WebApi.Database;

[AttributeUsage(AttributeTargets.Property)]
internal sealed class ExcludeFromAuditTrailAttribute : Attribute
{
}