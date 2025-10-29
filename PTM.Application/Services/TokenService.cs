
using PTM.Application.Interfaces.Authentication;
using PTM.Application.Interfaces.Repositories;
using PTM.Contracts.Response.Token;
using PTM.Domain.Models;

namespace PTM.Application.Interfaces.Services;

public class TokenService : ITokenService
{
    private readonly ITokenGenerator tokenGenerator;
    private readonly IRefreshTokenRepository refreshTokenRepository;
    private readonly IRequestContext requestContext;

    public TokenService(ITokenGenerator tokenGenerator,
     IRefreshTokenRepository refreshTokenRepository,
     IRequestContext requestContext)
    {
        this.tokenGenerator = tokenGenerator;
        this.refreshTokenRepository = refreshTokenRepository;
        this.requestContext = requestContext;
    }

    public string CreateAccessToken(User user, Guid jti)
    {
        return tokenGenerator.CreateAccessToken(user, jti);
    }
    public async Task<(string, RefreshToken)> CreateRefreshTokenAsync(User user)
    {
        var (rawToken, tokenHash, expiresAt) = tokenGenerator.CreateRefreshToken();
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = requestContext.GetIpAddress(),
            UserAgent = requestContext.GetUserAgent(),
            Jti = Guid.NewGuid()
        };
        await refreshTokenRepository.AddAsync(refreshToken);
        return (rawToken, refreshToken);
    }

    public async Task<TokenPair> GenerateTokenPair(User user)
    {
        var (raw, rt) = await CreateRefreshTokenAsync(user);
        var accessToken = CreateAccessToken(user, rt.Jti);
        return new TokenPair
        {
            AccessToken = accessToken,
            RefreshToken = raw
        };
    }

    public string HashToken(string rawToken)
    {
        return tokenGenerator.HashToken(rawToken);
    }
}
