using System;

namespace PTM.Contracts.Requests;

public class TaskItemUpdateRequest : BaseTaskItemRequest
{
    public Guid Id { get; set; }
}
