using System;
using FluentValidation;
using PTM.Contracts.Requests;

namespace PTM.Application.Validation.Validators.User;

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
        .NotEmpty().WithMessage("please enter an email")
        .EmailAddress().WithMessage("Please enter a valid email");
    }
}
