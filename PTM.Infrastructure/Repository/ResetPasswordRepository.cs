using System;
using Microsoft.EntityFrameworkCore;
using PTM.Application.Interfaces.Repositories;
using PTM.Domain.Models;
using PTM.Infrastructure.Database;

namespace PTM.Infrastructure.Repository;

public class ResetPasswordRepository : BaseRepository<ResetPassword>, IResetPasswordRepository
{
    private readonly AppDbContext context;

    public ResetPasswordRepository(AppDbContext context) : base(context)
    {
        this.context = context;
    }

    public async Task<ResetPassword?> GetResetPasswordByToken(string token, Guid userId)
    {
        return await context.ResetPasswords.FirstOrDefaultAsync(rp => rp.UserId == userId &&
         rp.Token == token && rp.Expires > DateTime.UtcNow);
    }
}
