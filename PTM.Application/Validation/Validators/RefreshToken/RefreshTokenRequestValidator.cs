using System;
using FluentValidation;
using PTM.Contracts.Requests;

namespace PTM.Application.Validation.Validators.RefreshToken;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>

{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.")
            .MinimumLength(20).WithMessage("Invalid refresh token format.");
    }

}
