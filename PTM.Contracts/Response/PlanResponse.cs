using System;
using PTM.Contracts.Requests;

namespace PTM.Contracts.Response;
public class PlanResponse
{
    public Guid Id { get; set; }
    public PlanTitle Title { get; set; } = PlanTitle.Free;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; } = 0;
    public int MaxTasks { get; set; }

}
