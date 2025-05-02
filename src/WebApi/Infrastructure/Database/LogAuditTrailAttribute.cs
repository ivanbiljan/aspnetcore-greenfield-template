using WebApi.Infrastructure.Database.Models;

namespace WebApi.Infrastructure.Database;

/// <summary>
///     Represents an attribute that instructs Entity Framework to produce detailed audit trails for the annotated entity.
///     When present, every modification is written to the database as an <see cref="AuditLog" /> entry.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
internal sealed class LogAuditTrailAttribute : Attribute
{
}