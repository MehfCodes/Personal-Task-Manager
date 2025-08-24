using System;
using System.Data;
using PTM.Application.Mappers;
using PTM.Contracts.Requests;
using PTM.Contracts.Response;
using PTM.Domain.Models;
using PTM.Infrastructure.Repository;

namespace PTM.Application.Services;

public class PlanService : IPlanService
{
    private readonly IBaseRepository<Plan> repository;

    public PlanService(IBaseRepository<Plan> repository)
    {
        this.repository = repository;
    }
    public async Task<PlanResponse> AddAsync(PlanRequest planRequest)
    {
        var newPlan = planRequest.MapToPlan();
        var record = await repository.AddAsync(newPlan);
        return record.MapToPlanResponse();
    }

    public async Task<PlanResponse?> GetByIdAsync(Guid id)
    {
        var record = await repository.GetByIdAsync(id);
        if (record is null) return null;
        return record.MapToPlanResponse();
    }
    public Task<bool> DeleteAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<PlanResponse>> GetAllAsync()
    {
        throw new NotImplementedException();
    }


    public Task<PlanResponse?> UpdateAsync(PlanUpdateRequest plan)
    {
        throw new NotImplementedException();
    }
}
