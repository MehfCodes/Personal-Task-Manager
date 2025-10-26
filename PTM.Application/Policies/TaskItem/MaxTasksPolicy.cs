using System;
using PTM.Application.Exceptions;
using PTM.Application.Interfaces.Policies;
using PTM.Application.Interfaces.Repositories;
using PTM.Contracts.Response.UserPlan;

namespace PTM.Application.Policies;

public class MaxTasksPolicy : ITaskItemPolicy
{
    private readonly ITaskItemRepository taskItemRepository;

    public MaxTasksPolicy(ITaskItemRepository taskItemRepository)
    {
        this.taskItemRepository = taskItemRepository;
    }
    public async Task Validate(Guid userId, UserPlanResponseDetail userPlan)
    {
        var numberOfTasks = await taskItemRepository.GetTaskCount(userId);
        var plan = userPlan.Plan;
        if (plan is not null && plan.MaxTasks <= numberOfTasks) throw new BusinessRuleException("You have reached the maximum number of tasks for your plan.");
    }
}
