using System;

namespace PTM.Domain.Models;

public enum Priority {Low, Mid, High}
public enum Status {Todo, InProgress, Done}
public class TaskItem
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string Description { get; set; } = string.Empty;
    public Status Status { get; set; }
    public Priority Priority { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid UserId { get; set; }
    public User? User { get; set; }
}
