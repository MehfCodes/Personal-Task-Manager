using System;
using FluentValidation;
using PTM.Contracts.Requests.TaskItem;
using PTM.Domain.Models;

namespace PTM.Application.Validation.Validators.TaskItem;

public class ChangePriorityValidator : AbstractValidator<ChangePriorityRequest>
{
public ChangePriorityValidator()
    {
        RuleFor(x => x.Priority)
        .NotEmpty().WithMessage("Please Enter the priority")
        .Must(priority=>
        Enum.TryParse<Priority>(priority, true, out var result) &&
        Enum.IsDefined(result)).WithMessage("Invalid priority value");
    }
}


