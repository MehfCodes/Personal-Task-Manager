using System;

namespace PTM.Domain.Models;

public class UserPlan
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public Guid PlanId { get; set; }
    public Plan? Plan { get; set; }
    public bool IsActive { get; set; }
    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiredAt { get; set; }
}
