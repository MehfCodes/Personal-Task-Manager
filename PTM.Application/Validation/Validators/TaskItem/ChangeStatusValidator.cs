using System;
using FluentValidation;
using PTM.Contracts.Requests.TaskItem;
using PTM.Domain.Models;

namespace PTM.Application.Validation.Validators.TaskItem;

public class ChangeStatusValidator : AbstractValidator<ChangeStatusRequest>
{
public ChangeStatusValidator()
    {
        RuleFor(x => x.Status)
        .NotEmpty().WithMessage("Please Enter the status")
        .Must(status=>
        Enum.TryParse<Status>(status, true, out var result) &&
        Enum.IsDefined(result))
        .WithMessage("Only Todo, InProgress and Done are allowed");
    }
}
