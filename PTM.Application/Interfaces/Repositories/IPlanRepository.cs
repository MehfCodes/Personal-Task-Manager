using System;
using PTM.Domain.Models;
using PTM.Infrastructure.Repository;

namespace PTM.Application.Interfaces.Repositories;

public interface IPlanRepository : IBaseRepository<Plan>
{
    Task DeActivePlan(Plan plan);
    Task ActivatePlan(Plan plan);
}
