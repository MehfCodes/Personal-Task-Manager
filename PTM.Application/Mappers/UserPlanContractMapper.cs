using System;
using PTM.Contracts.Response.UserPlan;
using PTM.Domain.Models;

namespace PTM.Application.Mappers;

public static class UserPlanContractMapper
{
    public static UserPlanResponse MapToUserPlanResponse(this UserPlan userPlan)
    {
        return new UserPlanResponse
        {
            Id = userPlan.Id,
            UserId = userPlan.UserId,
            PlanId = userPlan.PlanId,
            IsActive = userPlan.IsActive,
            PurchasedAt = userPlan.PurchasedAt,
            ExpiredAt = userPlan.ExpiredAt
        };
    }
}
