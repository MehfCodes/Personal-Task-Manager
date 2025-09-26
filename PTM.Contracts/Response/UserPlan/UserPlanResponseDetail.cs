using System;

namespace PTM.Contracts.Response.UserPlan;

public class UserPlanResponseDetail
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PlanId { get; set; }
    public bool IsActive { get; set; }
    public DateTime PurchasedAt { get; set; }
    public DateTime ExpiredAt { get; set; }
    public UserRes? User { get; set; }
    public PlanResponse? Plan { get; set; }
}
public class UserRes
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

