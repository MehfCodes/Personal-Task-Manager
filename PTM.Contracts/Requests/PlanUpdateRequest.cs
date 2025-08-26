using System;

namespace PTM.Contracts.Requests;

public class PlanUpdateRequest
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int MaxTasks { get; set; }
    public int DurationDays { get; set; }
    public bool IsActive { get; set; }
}
