using PTM.Application.Interfaces.Repositories;
using PTM.Application.Interfaces.Services;
using PTM.Contracts.Response.Token;
using PTM.Domain.Models;

namespace PTM.Application.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IRefreshTokenRepository repository;
    private readonly ITokenService tokenService;
    private readonly IRequestContext requestContext;
    public RefreshTokenService(IRefreshTokenRepository repository,
     ITokenService tokenService,
     IRequestContext requestContext)
    {
        this.repository = repository;
        this.tokenService = tokenService;
        this.requestContext = requestContext;
    }
    
    public async Task<RefreshToken?> GetRefreshToken(string token)
    {
        var tokenHash = tokenService.HashToken(token);
        return await repository.GetRefreshTokenByTokenHash(tokenHash);
    }
    public async Task<RevokeResult?> GenerateAndRevokeRefreshTokenAsync(string token)
    {
        var oldRefreshToken = await GetRefreshToken(token);
        if (oldRefreshToken == null || oldRefreshToken.RevokedAt != null || oldRefreshToken.User == null) return null;


        var (rawToken, newRefreshToken) = await tokenService.CreateRefreshTokenAsync(oldRefreshToken.User);
        await RevokeRefreshToken(oldRefreshToken, newRefreshToken.Id);

        var accessToken = tokenService.CreateAccessToken(oldRefreshToken.User, newRefreshToken.Jti);
        

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
            var ipAddress = requestContext.GetIpAddress() ?? "";
            var userAgent = requestContext.GetUserAgent() ?? "";
            var rt = await repository.GetRefreshTokenByUserId(userId, ipAddress, userAgent);
            if (rt is not null) await RevokeRefreshToken(rt);
        }
    } 
}
