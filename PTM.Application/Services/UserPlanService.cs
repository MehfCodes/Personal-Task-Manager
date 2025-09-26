using System;
using Microsoft.Extensions.Logging;
using PTM.Application.Exceptions;
using PTM.Application.Interfaces;
using PTM.Application.Interfaces.Repositories;
using PTM.Application.Interfaces.Services;
using PTM.Application.Mappers;
using PTM.Contracts.Response;
using PTM.Contracts.Response.UserPlan;
using PTM.Domain.Models;
using PTM.Infrastructure.Repository;

namespace PTM.Application.Services;

public class UserPlanService : BaseService, IUserPlanService
{
    private readonly IPlanRepository planRepository;
    private readonly IUserRepository userRepository;
    private readonly IBaseRepository<UserPlan> userPlanRepository;
    private readonly IRequestContext requestContext;
    private readonly ILogger<UserPlanService> logger;
    private Guid? userIdReq;
    public UserPlanService(IServiceProvider serviceProvider,
     IPlanRepository planRepository,
     IUserRepository userRepository,
     IRequestContext requestContext,
     ILogger<UserPlanService> logger,
     IBaseRepository<UserPlan> userPlanRepository) : base(serviceProvider)
    {
        this.planRepository = planRepository;
        this.userRepository = userRepository;
        this.userPlanRepository = userPlanRepository;
        this.requestContext = requestContext;
        this.logger = logger;
        userIdReq = requestContext.GetUserId();
    }

    public async Task<UserPlanResponseDetail> Purchase(Guid planId)
    {
        //deactive previous plans
        var activePlan = await HasActivePlan(userIdReq!.Value);
        if (activePlan) throw new BusinessRuleException("You already have a active plan, please deactive it and then purchase new one.");
        var plan = await planRepository.GetByIdAsync(planId);
        if (plan is null) throw new NotFoundException("Plan");
        var purchasedPlan = new UserPlan
        {
            UserId = userIdReq!.Value,
            PlanId = planId,
            IsActive = true,
            PurchasedAt = DateTime.UtcNow,
            ExpiredAt = DateTime.UtcNow.AddDays(plan.DurationDays)

        };
        await userPlanRepository.AddAsync(purchasedPlan);
        logger.LogInformation("User {UserId} purchased the Plan {PlanId} at {Time}", purchasedPlan.UserId, purchasedPlan.PlanId, DateTime.UtcNow);
        return purchasedPlan.MapToUserPlanWithPlanDetailResponse();
    }

    public async Task<UserPlanResponseDetail> GetUserPlanById(Guid userPlanId)
    {
        var up = await userPlanRepository.GetByIdAsync(userPlanId, up => up.User!, up => up.Plan!);
        if (up is null) throw new NotFoundException("Prchased plan");
        return up.MapToUserPlanWithPlanDetailResponse();
    }

    public async Task<UserPlanResponseDetail> GetActiveUserPlanByUserId(Guid userId)
    {
        var user = await userRepository.GetByIdAsync(userId, u => u.UserPlans);
        if (user is null) throw new NotFoundException("User");
        var activeUserPlan = user.UserPlans.Where(up => up.IsActive == true && up.ExpiredAt > DateTime.UtcNow).FirstOrDefault();
        if (activeUserPlan is null) throw new NotFoundException("User plan");
        return activeUserPlan.MapToUserPlanWithPlanDetailResponse();
    }
    public async Task<bool> HasActivePlan(Guid userId)
    {
        var user = await userRepository.GetByIdAsync(userId, u => u.UserPlans);
        if (user is null) throw new NotFoundException("User");
        var activeUserPlan = user.UserPlans.Where(up => up.IsActive == true && up.ExpiredAt > DateTime.UtcNow).FirstOrDefault();
        if (activeUserPlan is null) return false;
        return true;
    }

    public async Task<IEnumerable<UserPlanResponseDetail>> GetAllUserPlansByUserId(Guid userId)
    {
        var user = await userRepository.GetByIdAsync(userId, u => u.UserPlans);
        if (user is null) throw new NotFoundException("User");
        return user.UserPlans.ToList().MapToUserPlansResponse();
    }

    public async Task<IEnumerable<UserResponse>> GetAllUsersByPlanId(Guid planId)
    {
        var plan = await planRepository.GetByIdAsync(planId, p => p.UserPlans.Select(up => up.User));
        if (plan is null) throw new NotFoundException("Plan");
        var users = plan.UserPlans.Select(up => up.User).Where(u => u != null);
        return users!.MapToUsersResponse();
    }

    public async Task<MessageResponse> DeactivateAsync(Guid userPlanId)
    {
        var up = await userPlanRepository.GetByIdAsync(userPlanId);
        if (up is null) throw new NotFoundException("Prchased plan");
        if (!up.IsActive || up.ExpiredAt < DateTime.UtcNow) throw new BusinessRuleException("Your plan is already deactivated.");
        up.IsActive = false;
        await userPlanRepository.UpdateAsync(up);
        logger.LogInformation("User {UserId} deactive the Plan {PlanId} at {Time}", up.UserId, up.PlanId, DateTime.UtcNow);
        return new MessageResponse { Massage = "Plan deactivated." };
    }
}
