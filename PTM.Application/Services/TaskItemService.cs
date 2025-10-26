using System;
using PTM.Application.Exceptions;
using PTM.Application.Interfaces.Policies;
using PTM.Application.Interfaces.Repositories;
using PTM.Application.Interfaces.Services;
using PTM.Application.Mappers;
using PTM.Contracts.Requests;
using PTM.Contracts.Requests.TaskItem;
using PTM.Contracts.Response;
using PTM.Contracts.Response.TaskItem;
using PTM.Domain.Models;
using PTM.Infrastructure.Repository;

namespace PTM.Application.Services;

public class TaskItemService : BaseService, ITaskItemService
{
    private readonly ITaskItemRepository repository;
    private readonly IUserPlanService userPlanService;
    private readonly ICompositePolicy compositePolicy;
    private readonly IRequestContext requestContext;
    private readonly Guid? userIdReq;

    public TaskItemService(ITaskItemRepository repository,
     IUserPlanService userPlanService,
     IRequestContext requestContext,
     IServiceProvider serviceProvider, ICompositePolicy compositePolicy) : base(serviceProvider)
    {
        this.repository = repository;
        this.userPlanService = userPlanService;
        this.requestContext = requestContext;
        this.compositePolicy = compositePolicy;
    }
    public async Task<TaskItemResponse> AddAsync(TaskItemRequest request)
    {
        await ValidateAsync(request);
        var userId = requestContext.GetUserId()!.Value;
        var userPlan = await userPlanService.GetActiveUserPlanByUserId(userId);
        await compositePolicy.ValidateAll(userId, userPlan);
        var newTask = request.MapToTaskItem();
        var record = await repository.AddAsync(newTask);
        return record.MapToTaskItemResponse();
    }

    public async Task<IEnumerable<TaskItemResponse>> GetAllAsync()
    {
        var records = await repository.GetAllAsync();
        return records.MapToTaskItemsResponse();
    }

    public async Task<TaskItemResponse> GetByIdAsync(Guid id)
    {
        var record = await repository.GetByIdAsync(id);
        if (record is null) throw new NotFoundException("Task");
        return record.MapToTaskItemResponse();
    }

    public async Task<TaskItemResponse> UpdateAsync(Guid id, TaskItemUpdateRequest request)
    {
        await ValidateAsync(request);
        var record = await repository.GetByIdAsync(id);
        if (record is null) throw new NotFoundException("Task");
        request.Id = record.Id;
        var updated = request.MapToTaskItem(record);
        await repository.UpdateAsync(updated);
        return updated.MapToTaskItemResponse();
    }

    public async Task DeleteAsync(Guid id)
    {
        var record = await repository.DeleteAsync(id);
        if (record is null) throw new NotFoundException("Task");
    }

    public async Task<ChangeStatusResponse> ChangeStatus(Guid id, ChangeStatusRequest request)
    {
        await ValidateAsync(request);
        var task = await repository.GetByIdAsync(id);
        if (task is null) throw new NotFoundException("Task");
        Enum.TryParse<Status>(request.Status, true, out var newStatus);
        task.Status = newStatus;
        await repository.UpdateAsync(task);
        return new ChangeStatusResponse { Status = newStatus.ToString() };
    }
    public async Task<ChangePriorityResponse> ChangePriority(Guid id, ChangePriorityRequest request)
    {
        await ValidateAsync(request);
        var task = await repository.GetByIdAsync(id);
        if (task is null) throw new NotFoundException("Task");
        Enum.TryParse<Priority>(request.Priority, true, out var newPriority);
        task.Priority = newPriority;
        await repository.UpdateAsync(task);
        return new ChangePriorityResponse { Priority = newPriority.ToString() };
    }
}
