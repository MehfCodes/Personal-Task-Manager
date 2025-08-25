using System;
using PTM.Application.Interfaces.Repositories;
using PTM.Domain.Models;
using PTM.Infrastructure.Database;

namespace PTM.Infrastructure.Repository;

public class PlanRepository : BaseRepository<Plan> ,IPlanRepository
{
    private readonly AppDbContext context;

    public PlanRepository(AppDbContext context) : base(context)
    {
        this.context = context;
    }

    public async Task ActivatePlan(Plan plan)
    {
        plan.IsActive = true;
        await UpdateAsync(plan);
    }

    public async Task DeActivePlan(Plan plan)
    {
        plan.IsActive = false;
        await UpdateAsync(plan);
    }
}
