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
            Title = (Domain.Models.PlanTitle)planRequest.Title,
            Description = planRequest.Description,
            Price = planRequest.Price,
            MaxTasks = planRequest.MaxTasks
        };
    }
    public static PlanResponse MapToPlanResponse(this Plan plan) {
        return new PlanResponse
        {
            Id = plan.Id,
            Title = (Contracts.Requests.PlanTitle)plan.Title,
            Description = plan.Description,
            Price = plan.Price,
            MaxTasks = plan.MaxTasks
        };
    }
}
