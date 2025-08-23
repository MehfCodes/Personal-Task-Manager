using System;
using PTM.Contracts.Requests;
using PTM.Contracts.Response;
using PTM.Domain.Models;

namespace PTM.Application.Services;

public interface IPlanService
{
    Task<PlanResponse> AddAsync(PlanRequest plan);
    Task<PlanResponse?> GetByIdAsync(Guid id);
    Task<IEnumerable<PlanResponse>> GetAllAsync();
    Task<PlanResponse?> UpdateAsync(PlanUpdateRequest plan);
    Task<bool> DeleteAsync(Guid id);
}
