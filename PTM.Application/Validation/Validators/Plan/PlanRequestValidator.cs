using FluentValidation;
using PTM.Contracts.Requests;
using PTM.Domain.Models;

namespace PTM.Application.Validation.Validators.Plan;

public class PlanRequestValidator : AbstractValidator<PlanRequest>
{
    public PlanRequestValidator()
    {
       Include(new PlanValidatorRules());
    }
}
