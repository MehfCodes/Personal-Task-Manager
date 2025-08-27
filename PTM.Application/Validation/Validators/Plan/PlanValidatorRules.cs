using System;
using FluentValidation;
using PTM.Contracts.Requests;
using PTM.Domain.Models;

namespace PTM.Application.Validation.Validators.Plan;

public class PlanValidatorRules : AbstractValidator<BasePlanRequest>
{
    private bool CheckFreeTitle(BasePlanRequest x) =>
     string.Equals(x.Title, PlanTitle.Free.ToString(), StringComparison.OrdinalIgnoreCase);
    private bool CheckPremiumTitle(BasePlanRequest x) =>
     string.Equals(x.Title, PlanTitle.Premium.ToString(), StringComparison.OrdinalIgnoreCase);
    private bool CheckbusinessTitle(BasePlanRequest x) =>
     string.Equals(x.Title, PlanTitle.Business.ToString(), StringComparison.OrdinalIgnoreCase);
    public PlanValidatorRules()
    {
        RuleFor(x => x.Title)
        .NotEmpty().WithMessage("Please Enter the Title");

        RuleFor(x => x.Description)
        .NotNull().WithMessage("Please Enter the description");

        RuleFor(x => x.Price)
        .NotNull().WithMessage("Please Enter the price");

        RuleFor(x => x.Price)
        .GreaterThan(0).WithMessage("Price must be more than zero")
        .When(x => !CheckFreeTitle(x));

        RuleFor(x => x.Price)
        .Equal(0).WithMessage("Price must be zero for a free plan")
        .When(CheckFreeTitle);

        RuleFor(x => x.MaxTasks)
        .Must(value => value >= -1)
        .WithMessage("Enter non-negative number");

        RuleFor(x => x.MaxTasks)
        .InclusiveBetween(1, 10)
        .When(CheckFreeTitle)
        .WithMessage("Max Task must be between 0 and 10");

        RuleFor(x => x.MaxTasks)
        .GreaterThan(10)
        .When(CheckPremiumTitle)
        .WithMessage("Max Task must be more than 10");

        RuleFor(x => x.MaxTasks)
        .Equal(-1)
        .When(CheckbusinessTitle)
        .WithMessage("For business plan max task must be equal to -1");

        RuleFor(x => x.DurationDays)
        .NotNull().WithMessage("Please Enter the duration days");

        RuleFor(x => x.DurationDays)
        .Must(value => value > 0)
        .WithMessage("Enter natural number");

        RuleFor(x => x.DurationDays)
        .InclusiveBetween(1, 10)
        .When(CheckFreeTitle)
        .WithMessage("Duration of the free plan must be less than 10 days");

        RuleFor(x => x.DurationDays)
        .Equal(30)
        .When(CheckPremiumTitle)
        .WithMessage("Duration of the premium plan must be 30 days");

        RuleFor(x => x.DurationDays)
        .Equal(365)
        .When(CheckbusinessTitle)
        .WithMessage("Duration of the business plan must be 365 days");
    }


}
