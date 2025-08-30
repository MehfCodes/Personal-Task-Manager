using System;

namespace PTM.Contracts.Requests;

public class BaseTaskItemRequest
{
    public required string Title { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "ToDo";
    public string Priority { get; set; } = "Mid";
}
