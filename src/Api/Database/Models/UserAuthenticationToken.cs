using System.Diagnostics.CodeAnalysis;

namespace Api.Database.Models;

public sealed class UserAuthenticationToken : EntityBase<int>
{
    [SuppressMessage("Design", "CA1008:Enums should have zero value", Justification = "Not applicable")]
    public enum TokenPurpose
    {
        RefreshToken = 1,
        PasswordReset = 2,
        EmailConfirmation = 3,
        DeleteAccount = 4
    }

    public required TokenPurpose Purpose { get; init; }

    public required string Token { get; init; }

    public Instant? UsedOnUtc { get; set; }

    public User User { get; init; } = null!;

    public required int UserId { get; init; }

    public required Instant ValidUntilUtc { get; set; }
}