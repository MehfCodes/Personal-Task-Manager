using System;

namespace PTM.Contracts.Requests;

public class PlanRequest
{
    public string Title { get; set; } = "Free";
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int MaxTasks { get; set; } = -1;
    public int DurationDays { get; set; } = 7;
    public bool IsActive { get; set; } = true;
}
