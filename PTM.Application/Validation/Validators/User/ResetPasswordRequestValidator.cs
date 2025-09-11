using System;
using FluentValidation;
using PTM.Contracts.Requests.User;

namespace PTM.Application.Validation.Validators.User;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("token is required.")
            .Must(token => Guid.TryParseExact(token, "N", out _))
            .WithMessage("Token is invalid.");


        RuleFor(x => x.Email)
        .NotEmpty().WithMessage("please enter an email")
        .EmailAddress().WithMessage("Please enter a valid email");

        RuleFor(x => x.NewPassword)
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
