using System;
using PTM.Domain.Models;

namespace PTM.Application.Interfaces.Services;

public interface IRefreshTokenService
{
    Task<(string, RefreshToken)> CreateRefreshTokenAsync(User user, string ipAddress, string userAgent);
    Task<RefreshToken?> GetRefreshToken(string token);
    Task<RevokeResult?> RevokeRefreshTokenAsync(string token, User user, string ipAddress, string userAgent);
}

public class RevokeResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}