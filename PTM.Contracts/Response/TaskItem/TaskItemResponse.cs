using System;

namespace PTM.Contracts.Response;

public class TaskItemResponse
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Status { get; set; } 
    public string? Priority { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public UserResponse? User { get; set; }
}
