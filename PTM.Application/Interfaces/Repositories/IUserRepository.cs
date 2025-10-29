using System;
using PTM.Domain.Models;
using PTM.Infrastructure.Repository;

namespace PTM.Application.Interfaces;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetUserByEmail(string email);
    Task<User?> GetUserbyIdWithPlans(Guid id);
}
