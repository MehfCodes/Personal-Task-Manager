using System;
using PTM.Contracts.Response.UserPlan;

namespace PTM.Application.Interfaces.Services;

public interface IUserPlanService
{
    Task<UserPlanResponse> Purchase(Guid planId);
    Task<UserPlanResponse> GetUserPlanById(Guid userPlanId);
}
