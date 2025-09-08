using System;
using PTM.Domain.Models;
using PTM.Infrastructure.Repository;

namespace PTM.Application.Interfaces.Repositories;

public interface IResetPasswordRepository : IBaseRepository<ResetPassword>
{
    Task<ResetPassword?> GetResetPasswordByToken(string token, Guid userId);
}
