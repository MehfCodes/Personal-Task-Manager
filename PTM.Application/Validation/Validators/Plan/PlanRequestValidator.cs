using System;
using FluentValidation;
using PTM.Contracts.Requests;
using PTM.Domain.Models;

namespace PTM.Application.Validation.Validators.Plan;

public class PlanRequestValidator : AbstractValidator<PlanRequest>
{
    public PlanRequestValidator()
    {
        RuleFor(x => x.Title)
        .NotEmpty().WithMessage("Please Enter the Title");

        RuleFor(x => x.Description)
        .NotNull().WithMessage("Please Enter the description");

        RuleFor(x => x.Price)
        .NotNull().WithMessage("Please Enter the price");

        RuleFor(x => x.Price)
        .GreaterThan(0).WithMessage("Price must be more than zero")
        .When(x => x.Title != PlanTitle.Free.ToString());

        RuleFor(x => x.Price)
        .Equal(0).WithMessage("Price must be zero for a free plan")
        .When(x => x.Title == PlanTitle.Free.ToString());

        RuleFor(x => x.MaxTasks)
        .Must(value => value >= -1)
        .WithMessage("Enter non-negative number");

        RuleFor(x => x.MaxTasks)
        .InclusiveBetween(1, 10)
        .When(x => x.Title == PlanTitle.Free.ToString())
        .WithMessage("Max Task must be between 0 and 10");

        RuleFor(x => x.MaxTasks)
        .GreaterThan(10)
        .When(x => x.Title == PlanTitle.Premium.ToString())
        .WithMessage("Max Task must be more than 10");

        RuleFor(x => x.MaxTasks)
        .Equal(-1)
        .When(x => x.Title == PlanTitle.Business.ToString())
        .WithMessage("For business plan max task must be equal to -1");

        RuleFor(x => x.DurationDays)
        .NotNull().WithMessage("Please Enter the duration days");

        RuleFor(x => x.DurationDays)
        .Must(value => value > 0)
        .WithMessage("Enter natural number");

        RuleFor(x => x.DurationDays)
        .InclusiveBetween(1, 10)
        .When(x => x.Title == PlanTitle.Free.ToString())
        .WithMessage("Duration of the free plan must be less than 10 days");

        RuleFor(x => x.DurationDays)
        .Equal(30)
        .When(x => x.Title == PlanTitle.Premium.ToString())
        .WithMessage("Duration of the premium plan must be 30 days");

        RuleFor(x => x.DurationDays)
        .Equal(365)
        .When(x => x.Title == PlanTitle.Business.ToString())
        .WithMessage("Duration of the business plan must be 365 days");

    }
}
