using System;
using System.Data;
using FluentValidation;
using PTM.Application.Extentions;
using PTM.Application.Interfaces.Repositories;
using PTM.Application.Mappers;
using PTM.Application.Validation.Validators.Plan;
using PTM.Contracts.Requests;
using PTM.Contracts.Response;
using PTM.Domain.Models;
using PTM.Infrastructure.Repository;

namespace PTM.Application.Services;

public class PlanService : IPlanService
{
    private readonly IPlanRepository repository;
    private readonly IValidator<PlanRequest> createPlanValidator;
    private readonly IValidator<PlanUpdateRequest> updatePlanValidator;

    public PlanService(IPlanRepository repository,
     IValidator<PlanRequest> createPlanValidator,
     IValidator<PlanUpdateRequest> updatePlanValidator)
    {
        this.repository = repository;
        this.createPlanValidator = createPlanValidator;
        this.updatePlanValidator = updatePlanValidator;
    }
    public async Task<PlanResponse> AddAsync(PlanRequest planRequest)
    {
        await createPlanValidator.ValidateAndThrowAsync(planRequest);
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
    public async Task<IEnumerable<PlanResponse>> GetAllAsync()
    {
        var records = await repository.GetAllAsync();
        return records.MapToPlansResponse();
    }
    public async Task<PlanResponse?> UpdateAsync(Guid id, PlanUpdateRequest newPlan)
    {
        await updatePlanValidator.ValidateAndThrowAsync(newPlan);
        var record = await repository.GetByIdAsync(id);
        if (record is null) return null;
        newPlan.Id = record.Id;
        var updatedPlan = newPlan.MapToPlan(record);
        await repository.UpdateAsync(updatedPlan);
        return updatedPlan.MapToPlanResponse();
    }

    public async Task<bool> DeActiveAsync(Guid id)
    {
        var record = await repository.GetByIdAsync(id);
        if (record is null) return false;
        await repository.DeActivePlan(record);
        return true;
    }

    public async Task<bool> ActivateAsync(Guid id)
    {
        var record = await repository.GetByIdAsync(id);
        if (record is null) return false;
        await repository.ActivatePlan(record);
        return true;
    }
}
