using System;
using PTM.Contracts.Response.UserPlan;

namespace PTM.Application.Interfaces.Policies;

public interface ICompositePolicy
{
    Task ValidateAll(Guid userId, UserPlanResponseDetail userPlan);
}