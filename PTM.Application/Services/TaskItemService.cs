using System;
using PTM.Application.Mappers;
using PTM.Contracts.Requests;
using PTM.Contracts.Response;
using PTM.Domain.Models;
using PTM.Infrastructure.Repository;

namespace PTM.Application.Services;

public class TaskItemService : ITaskItemService
{
    private readonly IBaseRepository<TaskItem> repository;

    public TaskItemService(IBaseRepository<TaskItem> repository)
    {
        this.repository = repository;
    }
    public async Task<TaskItemResponse> AddAsync(TaskItemRequest request)
    {
        var newTask = request.MapToTakItem();
        var record = await repository.AddAsync(newTask);
        return record.MapToTakItemResponse();
    }

    public async Task<IEnumerable<TaskItemResponse>> GetAllAsync()
    {
        var records = await repository.GetAllAsync();
        return records.MapToTakItemsResponse();
    }

    public async Task<TaskItemResponse?> GetByIdAsync(Guid id)
    {
        var record = await repository.GetByIdAsync(id);
        if (record is null) return null;
        return record.MapToTakItemResponse();
    }

    public async Task<TaskItemResponse?> UpdateAsync(Guid id, TaskItemUpdateRequest request)
    {
        var record = await repository.GetByIdAsync(id);
        if (record is null) return null;
        request.Id = record.Id;
        var updated = request.MapToTakItem(record);
        await repository.UpdateAsync(updated);
        return updated.MapToTakItemResponse();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var record = await repository.DeleteAsync(id);
        if (record is null) return false;
        return true;
    }

}
