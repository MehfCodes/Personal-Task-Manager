using System;
using PTM.Application.Interfaces.Authentication;
using PTM.Application.Interfaces.Repositories;
using PTM.Application.Interfaces.Services;
using PTM.Domain.Models;
using PTM.Infrastructure.Repository;

namespace PTM.Application.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IRefreshTokenRepository repository;
    private readonly ITokenGenerator tokenGenerator;

    public RefreshTokenService(IRefreshTokenRepository repository, ITokenGenerator tokenGenerator)
    {
        this.repository = repository;
        this.tokenGenerator = tokenGenerator;
    }
    public async Task<(string, RefreshToken)> CreateRefreshTokenAsync(User user, string ipAddress, string userAgent)
    {
        var (rawToken, tokenHash, expiresAt) = tokenGenerator.CreateRefreshToken();
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress,
            UserAgent = userAgent
        };
        await repository.AddAsync(refreshToken);
        return (rawToken, refreshToken);
    }

    public async Task<RefreshToken?> GetRefreshToken(string token)
    {
        var tokenHash = tokenGenerator.HashRefreshToken(token);
        return await repository.GetRefreshTokenByTokenHash(tokenHash);
    }
    public async Task<RevokeResult?> RevokeRefreshTokenAsync(string token, User user, string ipAddress, string userAgent)
    {
        var oldRefreshToken = await GetRefreshToken(token);
        if (oldRefreshToken == null || oldRefreshToken.RevokedAt != null) return null;
        var (rawToken, newRefreshToken) = await CreateRefreshTokenAsync(user, ipAddress, userAgent);
        oldRefreshToken.RevokedAt = DateTime.UtcNow;
        oldRefreshToken.ReplacedByTokenId = newRefreshToken.Id;
        await repository.UpdateAsync(oldRefreshToken);
        var accessToken = tokenGenerator.CreateAccessToken(user);
        return new RevokeResult { AccessToken = accessToken, RefreshToken = rawToken };
        
    }
}
