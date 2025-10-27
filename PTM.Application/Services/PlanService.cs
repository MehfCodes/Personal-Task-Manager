
using PTM.Application.Exceptions;
using PTM.Application.Interfaces.Repositories;
using PTM.Application.Mappers;
using PTM.Contracts.Requests;
using PTM.Contracts.Response;
using PTM.Domain.Models;
using PTM.Infrastructure.Repository;
namespace PTM.Application.Services;

public class PlanService : BaseService, IPlanService
{
    private readonly IBaseRepository<Plan> repository;

    public PlanService(IBaseRepository<Plan> repository,
     IServiceProvider serviceProvider) : base(serviceProvider)
    {
        this.repository = repository;
    }
    public async Task<PlanResponse> AddAsync(PlanRequest planRequest)
    {
        await ValidateAsync(planRequest);
        var newPlan = planRequest.MapToPlan();
        var record = await repository.AddAsync(newPlan);
        return record.MapToPlanResponse();
    }

    public async Task<PlanResponse> GetByIdAsync(Guid id)
    {
        var record = await repository.GetByIdAsync(id);
        if (record is null) throw new NotFoundException("plan");
        return record.MapToPlanResponse();
    }
    public async Task<IEnumerable<PlanResponse>> GetAllAsync()
    {
        var records = await repository.GetAllAsync();
        return records.MapToPlansResponse();
    }
    public async Task<PlanResponse> UpdateAsync(Guid id, PlanUpdateRequest newPlan)
    {
        await ValidateAsync(newPlan);
        var record = await repository.GetByIdAsync(id);
        if (record is null) throw new NotFoundException("plan");
        newPlan.Id = record.Id;
        var updatedPlan = newPlan.MapToPlan(record);
        await repository.UpdateAsync(updatedPlan);
        return updatedPlan.MapToPlanResponse();
    }

    public async Task DeActiveAsync(Guid id)
    {
        var record = await repository.GetByIdAsync(id);
        if (record is null) throw new NotFoundException("plan");
        record.IsActive = false;
        await repository.UpdateAsync(record);
    }

    public async Task ActivateAsync(Guid id)
    {
        var record = await repository.GetByIdAsync(id);
        if (record is null) throw new NotFoundException("plan");
        record.IsActive = true;
        await repository.UpdateAsync(record);
    }
}
