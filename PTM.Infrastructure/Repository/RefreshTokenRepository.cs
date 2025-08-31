using System;
using Microsoft.EntityFrameworkCore;
using PTM.Application.Interfaces.Repositories;
using PTM.Domain.Models;
using PTM.Infrastructure.Database;

namespace PTM.Infrastructure.Repository;

public class RefreshTokenRepository : BaseRepository<RefreshToken>, IRefreshTokenRepository
{
    private readonly AppDbContext context;

    public RefreshTokenRepository(AppDbContext context) : base(context)
    {
        this.context = context;
    }

    public async Task<RefreshToken?> GetRefreshTokenByTokenHash(string tokenHash)
    {
        return await context.RefreshTokens.Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow);
    }
}
