using System;

namespace PTM.Contracts.Response.UserPlan;

public class UserPlanResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PlanId { get; set; }
    public bool IsActive { get; set; }
    public DateTime PurchasedAt { get; set; }
    public DateTime ExpiredAt { get; set; }
}
