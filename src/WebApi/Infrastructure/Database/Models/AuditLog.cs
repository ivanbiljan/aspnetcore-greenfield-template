using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WebApi.Infrastructure.Database.Models;

internal sealed class AuditLog : EntityBase<int>
{
    private static readonly JsonDocument EmptyDocument = JsonDocument.Parse("{}");
    
    public required string? Type { get; init; }

    public required string? TableName { get; init; }

    public required JsonDocument OldValues { get; init; } = EmptyDocument;

    public required JsonDocument NewValues { get; init; } = EmptyDocument;

    public required IEnumerable<string> AffectedColumns { get; init; } = [];

    public required JsonDocument PrimaryKey { get; init; } = EmptyDocument;

    public required string? Actor { get; init; }
    
    public required string? RequestPath { get; init; }
    
    public required string? TraceId { get; init; }
    
    public required string? SpanId { get; init; }
    
    public required JsonDocument ExtraProperties { get; init; } = EmptyDocument;
}

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.Property(l => l.Type)
            .HasMaxLength(16);

        builder.Property(l => l.TableName)
            .HasMaxLength(64);

        builder.Property(l => l.Actor)
            .HasMaxLength(64);

        builder.Property(l => l.RequestPath)
            .HasMaxLength(256);

        builder.Property(l => l.TraceId)
            .HasMaxLength(32);

        builder.Property(l => l.SpanId)
            .HasMaxLength(32);
    }
}