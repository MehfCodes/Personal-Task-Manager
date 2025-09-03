using System;
using PTM.Domain.Models;

namespace PTM.Application.Interfaces.Services;

public interface IRefreshTokenService
{
    Task<(string, RefreshToken)> CreateRefreshTokenAsync(User user);
    Task<RefreshToken?> GetRefreshToken(string token);
    Task<RevokeResult?> GenerateAndRevokeRefreshTokenAsync(string token);
    Task RevokePreviousToken(Guid userId);
}

public class RevokeResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}