using System;
using PTM.Contracts.Response;
using PTM.Contracts.Response.UserPlan;
using PTM.Domain.Models;

namespace PTM.Application.Interfaces.Services;

public interface IUserPlanService
{
    Task<UserPlanResponseDetail> Purchase(Guid planId);
    Task<UserPlanResponseDetail> GetUserPlanById(Guid userPlanId);
    Task<UserPlanResponseDetail> GetActiveUserPlanByUserId(Guid userId);
    Task<IEnumerable<UserPlanResponseDetail>> GetAllUserPlansByUserId(Guid userId);
    Task<IEnumerable<UserResponse>> GetAllUsersByPlanId(Guid planId);
    Task<MessageResponse> DeactivateAsync(Guid userPlanId);
    Task<UserPlan> AssignFreePLanToNewUser(Guid userId);
}
