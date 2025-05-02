namespace WebApi.Database.Models;

internal sealed class UserAuthenticationToken : EntityBase<int>
{
    public required int UserId { get; init; }

    public User User { get; init; } = null!;

    public required TokenPurpose Purpose { get; init; }

    public required Instant ValidUntilUtc { get; set; }

    public Instant? UsedOnUtc { get; set; }

    public required string Token { get; init; }

    internal enum TokenPurpose
    {
        RefreshToken = 1,
        PasswordReset = 2,
        EmailConfirmation = 3,
        DeleteAccount = 4
    }
}