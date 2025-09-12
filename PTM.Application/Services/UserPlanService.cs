using System;
using PTM.Application.Exceptions;
using PTM.Application.Interfaces.Repositories;
using PTM.Application.Interfaces.Services;
using PTM.Application.Mappers;
using PTM.Contracts.Response.UserPlan;
using PTM.Domain.Models;
using PTM.Infrastructure.Repository;

namespace PTM.Application.Services;

public class UserPlanService : BaseService, IUserPlanService
{
    private readonly IPlanRepository planRepository;
    private readonly IBaseRepository<UserPlan> userPlanRepository;
    private readonly IRequestContext requestContext;
    private Guid? userIdReq;
    public UserPlanService(IServiceProvider serviceProvider,
     IPlanRepository planRepository,
     IRequestContext requestContext,
     IBaseRepository<UserPlan> userPlanRepository) : base(serviceProvider)
    {
        this.planRepository = planRepository;
        this.userPlanRepository = userPlanRepository;
        this.requestContext = requestContext;
        userIdReq = requestContext.GetUserId();
    }

    public async Task<UserPlanResponse> Purchase(Guid planId)
    {
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
        return purchasedPlan.MapToUserPlanResponse();
    }

    public async Task<UserPlanResponse> GetUserPlanById(Guid userPlanId)
    {
        var up = await userPlanRepository.GetByIdAsync(userPlanId, up => up.User!, up => up.Plan!);
        if (up is null) throw new NotFoundException("Prchased plan");
        return up.MapToUserPlanResponse();
    }
}
