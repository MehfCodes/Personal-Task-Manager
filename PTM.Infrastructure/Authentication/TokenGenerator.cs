using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PTM.Application.Interfaces.Authentication;
using PTM.Domain.Models;


namespace PTM.Infrastructure.Authentication;

public class TokenGenerator : ITokenGenerator
{
    private readonly JwtOptions options;

    public TokenGenerator(IOptions<JwtOptions> options)
    {
        this.options = options.Value;
    }
    public string CreateAccessToken(User user, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email)
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(options.AccessTokenDuration),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public (string rawToken, string tokenHash, DateTime expiresAt) CreateRefreshToken()
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var hash = HashRefreshToken(rawToken);
        var expires = DateTime.UtcNow.AddDays(options.RefreshTokenDuration);
        return (rawToken, hash, expires);
    }

    public string HashRefreshToken(string rawToken)
    {
        return BCrypt.Net.BCrypt.HashPassword(rawToken);
    }
}
