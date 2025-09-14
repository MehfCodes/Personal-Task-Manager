using System;
using PTM.Domain.Models;
using PTM.Infrastructure.Repository;

namespace PTM.Application.Interfaces.Repositories;

public interface ITaskItemRepository :  IBaseRepository<TaskItem>
{
    Task<int> GetTaskCount(Guid userId);
}
