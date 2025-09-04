using System;
using PTM.Domain.Models;
using PTM.Infrastructure.Repository;

namespace PTM.Application.Interfaces.Repositories;

public interface IRefreshTokenRepository : IBaseRepository<RefreshToken>
{
    Task<RefreshToken?> GetRefreshTokenByTokenHash(string tokenHash);
    Task<RefreshToken?> GetRefreshTokenByUserId(Guid userId, string ipAddress, string userAgent, bool allDevice);
}
