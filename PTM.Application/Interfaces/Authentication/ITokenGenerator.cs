using System;
using PTM.Domain.Models;

namespace PTM.Application.Interfaces.Authentication;

public interface ITokenGenerator
{
    string CreateAccessToken(User user, Guid jti);
    (string rawToken, string tokenHash, DateTime expiresAt) CreateRefreshToken();
    string HashRefreshToken(string rawToken);
}

