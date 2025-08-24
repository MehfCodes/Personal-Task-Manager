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
            MaxTasks = planRequest.MaxTasks
        };
    }
    public static PlanResponse MapToPlanResponse(this Plan plan) {
        return new PlanResponse
        {
            Id = plan.Id,
            Title = plan.Title.ToString(),
            Description = plan.Description,
            Price = plan.Price,
            MaxTasks = plan.MaxTasks
        };
    }
}
