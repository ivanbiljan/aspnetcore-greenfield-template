namespace Api.Database;

[AttributeUsage(AttributeTargets.Property)]
internal sealed class ExcludeFromAuditTrailAttribute : Attribute
{
}