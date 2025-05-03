using System.Globalization;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using NodaTime.Extensions;

namespace WebApi.Features.Authentication;

[RegisterScoped]
internal sealed class AccessTokenManager(
    ApplicationDbContext context,
    JwtFactory jwtFactory,
    IOptions<JwtOptions> jwtOptions
)
{
    private readonly ApplicationDbContext _context = context;
    private readonly JwtFactory _jwtFactory = jwtFactory;
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<(string AccessToken, DateTime ExpiresAtUtc, string RefreshToken)> CreateAccessTokenAsync(
        int userId,
        CancellationToken cancellationToken = default
    )
    {
        var user = await _context.Users
            .Where(u => u.Id == userId)
            .SingleAsync(cancellationToken);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture))
        };

        var tokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationInMinutes);
        var accessToken = _jwtFactory.Create(claims, expiresAt: tokenExpiresAt);

        var refreshToken = Guid.NewGuid().ToString();
        var refreshTokenExpiration = DateTime.UtcNow.AddMinutes(_jwtOptions.RefreshTokenExpirationInMinutes);
        user.AuthenticationTokens.Add(
            new UserAuthenticationToken
            {
                UserId = user.Id,
                Purpose = UserAuthenticationToken.TokenPurpose.RefreshToken,
                Token = refreshToken,
                ValidUntilUtc = tokenExpiresAt.ToInstant()
            }
        );

        return (accessToken, tokenExpiresAt, refreshToken);
    }

    public async Task InvalidateRefreshTokensAsync(int userId, CancellationToken cancellationToken = default)
    {
        var instant = DateTime.UtcNow.ToInstant();
        var refreshTokens = await _context.UserAuthenticationTokens
            .Where(t => t.UserId == userId)
            .Where(t => t.Purpose == UserAuthenticationToken.TokenPurpose.RefreshToken)
            .Where(t => t.ValidUntilUtc > instant)
            .ToListAsync(cancellationToken);

        foreach (var refreshToken in refreshTokens)
        {
            refreshToken.ValidUntilUtc = instant;
        }
    }
}