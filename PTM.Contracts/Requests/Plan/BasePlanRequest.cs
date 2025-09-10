using System;

namespace PTM.Contracts.Requests;

public abstract class BasePlanRequest
{
    public string? Title { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int MaxTasks { get; set; } = -1;
    public int DurationDays { get; set; } = 7;
    public bool IsActive { get; set; } = true;
}
