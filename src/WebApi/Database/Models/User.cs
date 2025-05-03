using System.Collections.ObjectModel;

namespace WebApi.Database.Models;

public sealed class User : EntityBase<int>
{
    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public required LocalDate DateOfBirth { get; set; }

    public required string Email { get; set; }

    public required string Password { get; set; }

    public Instant? EmailConfirmedAtUtc { get; set; }

    public Collection<UserAuthenticationToken> AuthenticationTokens { get; } = [];
}