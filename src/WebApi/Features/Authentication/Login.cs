using System.Text.Json.Serialization;
using Destructurama.Attributed;
using Immediate.Apis.Shared;
using Immediate.Handlers.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace WebApi.Features.Authentication;

[Handler]
[MapPost("/api/account/login")]
[AllowAnonymous]
internal static partial class Login
{
    private static async ValueTask<Response> HandleAsync(
        Command command,
        ApplicationDbContext context,
        IPasswordHasher<User> passwordHasher,
        AccessTokenManager accessTokenManager,
        ILogger<Command> logger,
        CancellationToken cancellationToken
    )
    {
        var user = await context.Users
            .Where(a => a.Email == command.Email)
            .SingleOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Invalid email or password");

        using (logger.BeginScope(("User", user.Id)))
        {
            if (!user.EmailConfirmedAtUtc.HasValue)
            {
                logger.LogInformation("Login denied because email has not been confirmed");

                throw new NotFoundException("Invalid email or password");
            }

            if (passwordHasher.VerifyHashedPassword(null!, user.Password!, command.Password) is not
                PasswordVerificationResult.Success)
            {
                logger.LogInformation("Login denied due to invalid credentials");

                throw new NotFoundException("Invalid email or password");
            }

            var accessToken = await accessTokenManager.CreateAccessToken(user.Id, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);

            return new Response
            {
                AccessToken = accessToken.AccessToken,
                ExpiresAtUtc = (long) accessToken.ExpiresAtUtc.Subtract(DateTime.UnixEpoch).TotalSeconds,
                RefreshToken = accessToken.RefreshToken
            };
        }
    }

    internal sealed record Command
    {
        [LogMasked]
        [JsonPropertyName("email")]
        public required string Email { get; init; }

        [LogMasked]
        [JsonPropertyName("password")]
        public required string Password { get; init; }
    }

    internal sealed record Response
    {
        [JsonPropertyName("accessToken")]
        public required string AccessToken { get; init; }

        [JsonPropertyName("expiresAtUtc")]
        public required long ExpiresAtUtc { get; init; }

        [JsonPropertyName("refreshToken")]
        public required string RefreshToken { get; init; }
    }
}