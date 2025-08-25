using System;

namespace PTM.Domain.Models;

public enum PlanTitle {Free, Premium, Business};
public class Plan
{
    public Guid Id { get; set; }
    public PlanTitle Title { get; set; } = PlanTitle.Free;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int MaxTasks { get; set; } = -1;
    public bool IsActive { get; set; } = true;
    public ICollection<User> Users { get; set; } = [];
}
