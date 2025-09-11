using System;
using PTM.Contracts.Requests;
using PTM.Contracts.Response;

namespace PTM.Application.Mappers;

public interface ITaskItemService
{
    Task<TaskItemResponse> AddAsync(TaskItemRequest taskItem);
    Task<TaskItemResponse> GetByIdAsync(Guid id);
    Task<IEnumerable<TaskItemResponse>> GetAllAsync();
    Task<TaskItemResponse> UpdateAsync(Guid id, TaskItemUpdateRequest taskItem);
    Task DeleteAsync(Guid id);
}
