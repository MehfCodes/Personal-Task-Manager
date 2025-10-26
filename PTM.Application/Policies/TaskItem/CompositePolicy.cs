using System;
using PTM.Application.Interfaces.Policies;
using PTM.Contracts.Response.UserPlan;

namespace PTM.Application.Policies;

public class CompositePolicy : ICompositePolicy
{
    private readonly IEnumerable<ITaskItemPolicy> policies;

    public CompositePolicy(IEnumerable<ITaskItemPolicy> policies)
    {
        this.policies = policies;
    }
    public async Task ValidateAll(Guid userId, UserPlanResponseDetail userPlan)
    {
        foreach (var policy in policies)
            await policy.Validate(userId, userPlan);
    }
}
