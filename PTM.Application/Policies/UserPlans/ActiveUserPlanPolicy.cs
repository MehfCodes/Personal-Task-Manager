using System;
using PTM.Application.Exceptions;
using PTM.Application.Interfaces;
using PTM.Application.Interfaces.Policies;
using PTM.Domain.Models;

namespace PTM.Application.Policies.UserPlans;

public class ActiveUserPlanPolicy : IUserPlanPolicy<Guid>
{
    private readonly IUserRepository userRepository;

    public ActiveUserPlanPolicy(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task Validate(Guid userId)
    {
        var user = await userRepository.GetUserbyIdWithPlans(userId);
        if (user is null) throw new NotFoundException("User");
        var activeUserPlan = user.UserPlans.Any(up => up.IsActive == true && up.ExpiredAt > DateTime.UtcNow && up.Plan!.Title.ToString() != "Free");
        if (activeUserPlan) throw new BusinessRuleException("You already have a active plan, please deactive it and then purchase new one.");
    }
}
