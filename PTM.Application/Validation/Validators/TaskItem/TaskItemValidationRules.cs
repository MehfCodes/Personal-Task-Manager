using System;
using FluentValidation;
using PTM.Contracts.Requests;
using PTM.Domain.Models;

namespace PTM.Application.Validation.Validators.TaskItem;

public class TaskItemValidationRules : AbstractValidator<BaseTaskItemRequest>
{
    public TaskItemValidationRules()
    {
        
        RuleFor(x => x.Title)
        .NotEmpty().WithMessage("Please Enter the Title");

        RuleFor(x => x.Description)
        .NotNull().WithMessage("Please Enter the description");

        RuleFor(x => x.Priority)
        .NotEmpty().WithMessage("Please Enter the priority")
        .Must(priority=>
        Enum.TryParse<Priority>(priority, true, out var result) &&
        Enum.IsDefined(result)).WithMessage("Invalid priority value");
        

        RuleFor(x => x.Status)
        .NotEmpty().WithMessage("Please Enter the status")
        .Must(status=>
        Enum.TryParse<Status>(status, true, out var result) &&
        Enum.IsDefined(result))
        .WithMessage("Only Todo, InProgress and Done are allowed");
    }
}
