using System.Collections.ObjectModel;

namespace Api.Database.Models;

public sealed class User : EntityBase<int>
{
    public Collection<UserAuthenticationToken> AuthenticationTokens { get; } = [];

    public required LocalDate DateOfBirth { get; set; }

    public required string Email { get; set; }

    public Instant? EmailConfirmedAtUtc { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public required string Password { get; set; }
}