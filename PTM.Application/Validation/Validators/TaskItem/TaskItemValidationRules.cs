using System;
using FluentValidation;
using PTM.Contracts.Requests;
using PTM.Domain.Models;

namespace PTM.Application.Validation.Validators.TaskItem;

public class TaskItemValidationRules : AbstractValidator<BaseTaskItemRequest>
{
    public TaskItemValidationRules()
    {
        var allowedStatuses = new[] { "Todo", "InProgress", "Done" };
        var allowedPriority = new[] { "Low", "Mid", "High" };

        RuleFor(x => x.Title)
        .NotEmpty().WithMessage("Please Enter the Title");

        RuleFor(x => x.Description)
        .NotNull().WithMessage("Please Enter the description");

        RuleFor(x => x.Priority)
        .NotEmpty().WithMessage("Please Enter the priority")
        .Must(value => allowedPriority.Contains(value))
        .WithMessage("Invalid priority value");


        RuleFor(x => x.Status)
        .NotEmpty().WithMessage("Please Enter the status")
        .Must(value =>  allowedStatuses.Contains(value))
        .WithMessage("Only Todo, InProgress and Done are allowed");
    }
}
