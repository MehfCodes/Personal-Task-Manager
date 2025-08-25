using System;
using PTM.Contracts.Requests;
using PTM.Contracts.Response;
using PTM.Domain.Models;

namespace PTM.Application.Mappers;

public static class PlanContractMapper
{
    public static Plan MapToPlan(this PlanRequest planRequest) {
        
        return new Plan
        {
            Title = Enum.TryParse(planRequest.Title, true, out PlanTitle ParsedTitle) ? ParsedTitle : PlanTitle.Free,
            Description = planRequest.Description,
            Price = planRequest.Price,
            MaxTasks = planRequest.MaxTasks,
            IsActive = planRequest.IsActive
        };
    }
    public static Plan MapToPlan(this PlanUpdateRequest planRequest, Plan plan) {
        plan.Id = planRequest.Id;
        plan.Title = Enum.TryParse(planRequest.Title, true, out PlanTitle ParsedTitle) ? ParsedTitle : PlanTitle.Free;
        plan.Description = planRequest.Description ?? "";
        plan.Price = planRequest.Price;
        plan.MaxTasks = planRequest.MaxTasks;
        plan.IsActive = planRequest.IsActive;
        return plan;
    }
    public static PlanResponse MapToPlanResponse(this Plan plan) {
        return new PlanResponse
        {
            Id = plan.Id,
            Title = plan.Title.ToString(),
            Description = plan.Description,
            Price = plan.Price,
            MaxTasks = plan.MaxTasks,
            IsActive = plan.IsActive
        };
    }
    public static IEnumerable<PlanResponse> MapToPlansResponse(this IEnumerable<Plan> plans) => plans.Select(MapToPlanResponse);
}
