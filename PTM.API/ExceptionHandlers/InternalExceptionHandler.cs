using System;

namespace PTM.API.ExceptionHandlers;

public class InternalExceptionHandler : ExceptionHandlerBase<Exception>
{
    protected override int GetStatusCode(Exception ex) => StatusCodes.Status500InternalServerError;

    protected override string GetMessage(Exception ex) => "An unexpected error occurred. Please try again later.";
}