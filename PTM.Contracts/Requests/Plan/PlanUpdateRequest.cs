using System;

namespace PTM.Contracts.Requests;

public class PlanUpdateRequest : BasePlanRequest
{
    public Guid Id { get; set; }

}
