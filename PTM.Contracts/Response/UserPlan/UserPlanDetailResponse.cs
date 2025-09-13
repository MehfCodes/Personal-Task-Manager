using System;

namespace PTM.Contracts.Response.UserPlan;

public class UserPlanDetailResponse : UserPlanResponse
{
    public UserResponse? User { get; set; }
    public PlanResponse? Plan { get; set; }
}
