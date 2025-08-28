using System;
using FluentValidation;
using PTM.Contracts.Requests;

namespace PTM.Application.Validation.Validators.User;

public class UserRequestValidator : AbstractValidator<UserRequest>
{
    public UserRequestValidator()
    {
        Include(new UserValidationRules());

        RuleFor(x => x.Password)
        .NotEmpty()
        .WithMessage("please enter a password")
        .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
        .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
        .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
        .Matches(@"\d").WithMessage("Password must contain at least one number")
        .Matches(@"[\W_]").WithMessage("Password must contain at least one special character")
        .Matches(@"^\S+$").WithMessage("Password cannot contain spaces");

    }
}
