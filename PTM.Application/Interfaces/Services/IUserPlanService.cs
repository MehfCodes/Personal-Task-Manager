using System;
using PTM.Contracts.Response;
using PTM.Contracts.Response.UserPlan;

namespace PTM.Application.Interfaces.Services;

public interface IUserPlanService
{
    Task<UserPlanResponse> Purchase(Guid planId);
    Task<UserPlanResponse> GetUserPlanById(Guid userPlanId);
    Task<UserPlanResponse> GetActiveUserPlanByUserId(Guid userId);
    Task<IEnumerable<UserPlanResponse>> GetAllUserPlansByUserId(Guid userId);
    Task<IEnumerable<UserResponse>> GetAllUsersByPlanId(Guid planId);
    Task<UserPlanResponse> DeactivateAsync(Guid userPlanId);
}
