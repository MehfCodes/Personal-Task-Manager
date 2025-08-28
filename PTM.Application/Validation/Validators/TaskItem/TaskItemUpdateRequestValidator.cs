using System;
using FluentValidation;
using PTM.Contracts.Requests;

namespace PTM.Application.Validation.Validators.TaskItem;

public class TaskItemUpdateRequestValidator : AbstractValidator<TaskItemUpdateRequest>
{
    public TaskItemUpdateRequestValidator()
    {
        Include(new TaskItemValidationRules());
    }
}
