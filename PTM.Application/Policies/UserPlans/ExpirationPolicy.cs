using PTM.Application.Exceptions;
using PTM.Application.Interfaces.Policies;
using PTM.Domain.Models;

namespace PTM.Application.Policies.UserPlans;

public class ExpirationPolicy : IUserPlanPolicy<UserPlan>
{
    public Task Validate(UserPlan userPlan)
    {
        if (userPlan.ExpiredAt < DateTime.UtcNow || !userPlan.IsActive)
            throw new BusinessRuleException("The Plan has expired.");
        return Task.CompletedTask;
    }
}
