using System;
using PTM.Application.Exceptions;
using PTM.Application.Interfaces.Policies;
using PTM.Application.Interfaces.Services;
using PTM.Contracts.Response.UserPlan;

namespace PTM.Application.Policies;

public class ActivePlanPolicy : ITaskItemPolicy
{
    private readonly IUserPlanService userPlanService;

    public ActivePlanPolicy(IUserPlanService userPlanService)
    {
        this.userPlanService = userPlanService;
    }
    public async Task Validate(Guid userId, UserPlanResponseDetail userPlan)
    {
        if (userPlan is null || userPlan.Plan is null) throw new BusinessRuleException("You don't have plan");
    }
}
