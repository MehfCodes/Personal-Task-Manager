using System;
using FluentValidation;
using PTM.Contracts.Requests;

namespace PTM.Application.Validation.Validators.User;

public class UserValidationRules : AbstractValidator<BaseUserRequest>
{
    public UserValidationRules()
    {
        RuleFor(x => x.Username)
        .NotEmpty().WithMessage("please choose a valid username")
        .MinimumLength(4).WithMessage("username length must be at least 4 characters");


        RuleFor(x => x.Email)
        .NotEmpty().WithMessage("please enter an email")
        .EmailAddress().WithMessage("Please enter a valid email");

        RuleFor(x => x.PhoneNumber)
        .NotEmpty().WithMessage("please enter a phone number")
        .Matches(@"^09\d{9}$").WithMessage("Phone number must be 11 digits");
    }
}
