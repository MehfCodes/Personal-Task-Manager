using System;
using PTM.Contracts.Requests;
using PTM.Contracts.Response;
using PTM.Domain.Models;

namespace PTM.Application.Mappers;

public static class TaskItemContractMapper
{
    public static TaskItem MapToTaskItem(this TaskItemRequest request)
    {
        return new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            Status = Enum.TryParse(request.Status, true, out Status status) ? status : Status.Todo,
            Priority = Enum.TryParse(request.Priority, true, out Priority priority) ? priority : Priority.Mid,
        };
    }
    public static TaskItem MapToTaskItem(this TaskItemUpdateRequest request, TaskItem taskItem)
{       taskItem.Title = request.Title;
        taskItem.Description = request.Description;
        taskItem.Status = Enum.TryParse(request.Status, true, out Status status) ? status : Status.Todo;
        taskItem.Priority = Enum.TryParse(request.Priority, true, out Priority priority) ? priority : Priority.Mid;
        return taskItem;
    }
    public static TaskItemResponse MapToTaskItemResponse(this TaskItem taskItem)
    {
        return new TaskItemResponse
        {
            Id = taskItem.Id,
            Title = taskItem.Title,
            Description = taskItem.Description,
            Status = taskItem.Status.ToString(),
            Priority = taskItem.Priority.ToString()
        };
    }
    public static IEnumerable<TaskItemResponse> MapToTaskItemsResponse(this IEnumerable<TaskItem> taskItem) => taskItem.Select(MapToTaskItemResponse);
  
}
