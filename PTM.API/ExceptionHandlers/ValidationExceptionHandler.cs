using System;
using PTM.Application.Exceptions;

namespace PTM.API.ExceptionHandlers;

public class ValidationExceptionHandler : ExceptionHandlerBase<ValidationException>
{
    protected override int GetStatusCode(ValidationException ex) => StatusCodes.Status400BadRequest;


    protected override string GetMessage(ValidationException ex) => ex.Message;


    protected override IDictionary<string, string[]>? GetErrors(ValidationException ex)
    {
        return ex.Errors.GroupBy(e => e.PropertyName).ToDictionary(
            k => k.Key,
            v => v.Select(e => e.ErrorMessage).ToArray()
        );        
    }
        
}