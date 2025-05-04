using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SourceGenerators;

namespace Api.Features.Authentication;

[ConfigureOptions(ConfigurationSectionName)]
internal sealed record JwtOptions
{
    public const string ConfigurationSectionName = "Jwt";

    [Required]
    public required IEnumerable<string> Audience { get; init; } = [];

    [Required]
    public required string Issuer { get; init; }

    [Required]
    public required string Key { get; init; }

    public required int AccessTokenExpirationInMinutes { get; init; } = 5;

    public required int RefreshTokenExpirationInMinutes { get; init; } = 24 * 60;
}

[RegisterSingleton]
internal sealed class JwtFactory
{
    private readonly JwtOptions _options;

    public JwtFactory(IOptions<JwtOptions> jwtOptions)
    {
        // This prevents Microsoft from overriding claim names (e.g. mapping "sub" to "https://schemas.xmlsoap.org/ws/...).
        JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();
        _options = jwtOptions.Value;
    }

    public string Create(IEnumerable<Claim> claims, DateTime? notBefore = null, DateTime? expiresAt = null)
    {
        var jwtHandler = new JsonWebTokenHandler();

        var claimList = new List<Claim>(claims)
            .Union(_options.Audience.Select(audience => new Claim(JwtRegisteredClaimNames.Aud, audience)))
            .GroupBy(c => c.Type)
            .ToDictionary(
                g => g.Key,
                g => (object) (g.Count() == 1 ? g.First().Value : g.Select(c => c.Value).ToArray())
            );

        var securityTokenDescription = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            IssuedAt = DateTime.UtcNow,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_options.Key)),
                SecurityAlgorithms.HmacSha256
            ),
            Claims = claimList,
            NotBefore = notBefore,
            Expires = expiresAt
        };

        return jwtHandler.CreateToken(securityTokenDescription);
    }
}