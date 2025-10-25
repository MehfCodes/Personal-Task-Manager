using PTM.Contracts.Response.Token;
using PTM.Domain.Models;

namespace PTM.Application.Interfaces.Services;

public interface IRefreshTokenService
{
    Task<RefreshToken?> GetRefreshToken(string token);
    Task<RevokeResult?> GenerateAndRevokeRefreshTokenAsync(string token);
    Task RevokePreviousToken(Guid userId, bool allDevice = false);
}