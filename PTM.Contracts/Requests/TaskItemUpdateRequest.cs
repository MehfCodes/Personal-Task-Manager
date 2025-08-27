using System;

namespace PTM.Contracts.Requests;

public class TaskItemUpdateRequest
{
    public required string Title { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Status { get; set; }
    public string? Priority { get; set; }
}
