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
    private string ipAddress;
    private string userAgent;
    public RefreshTokenService(IRefreshTokenRepository repository,
     ITokenGenerator tokenGenerator,
     IRequestContext requestContext)
    {
        this.repository = repository;
        this.tokenGenerator = tokenGenerator;
        ipAddress = requestContext.GetIpAddress() ?? "";
        userAgent = requestContext.GetUserAgent() ?? "";
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
            CreatedByIp = ipAddress,
            UserAgent = userAgent,
            Jti = Guid.NewGuid()
        };
        await repository.AddAsync(refreshToken);
        return (rawToken, refreshToken);
    }

    public async Task<RefreshToken?> GetRefreshToken(string token)
    {
        var tokenHash = tokenGenerator.HashToken(token);
        return await repository.GetRefreshTokenByTokenHash(tokenHash);
    }
    public async Task<RevokeResult?> GenerateAndRevokeRefreshTokenAsync(string token)
    {
        var oldRefreshToken = await GetRefreshToken(token);
        if (oldRefreshToken == null || oldRefreshToken.RevokedAt != null || oldRefreshToken.User == null) return null;


        var (rawToken, newRefreshToken) = await CreateRefreshTokenAsync(oldRefreshToken.User);
        await RevokeRefreshToken(oldRefreshToken, newRefreshToken.Id);

        var accessToken = tokenGenerator.CreateAccessToken(oldRefreshToken.User, newRefreshToken.Jti);
        

        return new RevokeResult { AccessToken = accessToken, RefreshToken = rawToken };

    }
    
    private async Task RevokeRefreshToken(RefreshToken refreshToken, Guid? newRefreshTokenId = null)
    {
        refreshToken.RevokedAt = DateTime.UtcNow;
        if (newRefreshTokenId is not null) refreshToken.ReplacedByTokenId = newRefreshTokenId;
        await repository.UpdateAsync(refreshToken);
    }
    
    public async Task RevokePreviousToken(Guid userId, bool allDevice = false)
    {
        if (allDevice)
        {
            var rts = await repository.GetRefreshTokensByUserId(userId);
            foreach(var rt in rts)
            {
                await RevokeRefreshToken(rt);
            };
        }
        else
        {
            var rt = await repository.GetRefreshTokenByUserId(userId, ipAddress, userAgent);
            if (rt is not null) await RevokeRefreshToken(rt);
        }
    } 
}
