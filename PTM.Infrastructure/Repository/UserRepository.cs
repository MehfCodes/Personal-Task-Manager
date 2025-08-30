using System;
using Microsoft.EntityFrameworkCore;
using PTM.Application.Interfaces;
using PTM.Domain.Models;
using PTM.Infrastructure.Database;

namespace PTM.Infrastructure.Repository;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    private readonly AppDbContext context;

    public UserRepository(AppDbContext context) : base(context)
    {
        this.context = context;
    }

    public async Task<User?> GetUserByEmail(string email) => await context.Users.FirstOrDefaultAsync(u => u.Email == email);
}
