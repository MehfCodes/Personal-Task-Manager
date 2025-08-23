using System;

namespace PTM.Contracts.Requests;

public class PlanUpdateRequest
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "Free";
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int MaxTasks { get; set; } = -1;
}
