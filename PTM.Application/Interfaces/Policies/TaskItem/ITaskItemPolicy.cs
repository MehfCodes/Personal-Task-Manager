using PTM.Contracts.Response.UserPlan;

namespace PTM.Application.Interfaces.Policies;

public interface ITaskItemPolicy
{
    Task Validate(Guid userId, UserPlanResponseDetail userPlan);
}
