using System;
using FluentValidation;
using PTM.Contracts.Requests;

namespace PTM.Application.Validation.Validators.TaskItem;

public class TaskItemRequestValidator : AbstractValidator<TaskItemRequest>
{
    public TaskItemRequestValidator()
    {
        Include(new TaskItemValidationRules());
    }
}
