using System;
using PTM.Application.Interfaces.Authentication;
using PTM.Contracts.Response.Token;
using PTM.Domain.Models;

namespace PTM.Application.Interfaces.Services;

public interface ITokenService
{
    string HashToken(string rawToken);
    Task<TokenPair> GenerateTokenPair(User user);
    Task<(string, RefreshToken)> CreateRefreshTokenAsync(User user);
    string CreateAccessToken(User user, Guid jti);
}
