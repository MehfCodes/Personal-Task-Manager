using System;

namespace PTM.Contracts.Requests;
public enum PlanTitle {Free, premium, Business};
public class PlanRequest
{
    public PlanTitle Title { get; set; } = PlanTitle.Free;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int MaxTasks { get; set; } = -1;

}
