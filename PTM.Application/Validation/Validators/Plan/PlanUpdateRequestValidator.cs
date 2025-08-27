using System;
using FluentValidation;
using PTM.Contracts.Requests;

namespace PTM.Application.Validation.Validators.Plan;

public class PlanUpdateRequestValidator : AbstractValidator<PlanUpdateRequest>
{
    public PlanUpdateRequestValidator()
    {
        Include(new PlanValidatorRules());
    }
}
