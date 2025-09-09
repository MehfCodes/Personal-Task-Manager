using System;
using PTM.Application.Exceptions;

namespace PTM.API.ExceptionHandlers;

public class ValidationExceptionHandler : ExceptionHandlerBase<ValidationException>
{
    protected override int GetStatusCode(ValidationException ex) => StatusCodes.Status400BadRequest;
        

    protected override string GetMessage(ValidationException ex) => "Validation failed.";
        

    protected override IDictionary<string, string[]>? GetErrors(ValidationException ex) => ex.Errors;
        
}