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
    private readonly HashSecret secret;

    public TokenGenerator(IOptions<JwtOptions> options, IOptions<HashSecret> secret)
    {
        this.options = options.Value;
        this.secret = secret.Value;
    }
    public string CreateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
        };

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
        var tokenId = Guid.NewGuid().ToString("N");
        var randomPart = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var rawToken = $"{tokenId}.{randomPart}";
        var hash = HashRefreshToken(rawToken);
        var expires = DateTime.UtcNow.AddDays(options.RefreshTokenDuration);
        return (rawToken, hash, expires);
    }

    public string HashRefreshToken(string rawToken)
    {
        var bytes = Encoding.UTF8.GetBytes(rawToken + secret.Key);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

}
