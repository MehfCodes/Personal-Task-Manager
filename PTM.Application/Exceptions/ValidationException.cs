using System;
using FluentValidation.Results;

namespace PTM.Application.Exceptions;

public class ValidationException : Exception
{
    public IEnumerable<ValidationFailure> Errors { get; set; }

    public ValidationException(IEnumerable<ValidationFailure> errors) : base("Validation failed.")
    {
        Errors = errors;
    }
}
