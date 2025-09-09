using System;
using PTM.Application.Exceptions;

namespace PTM.API.ExceptionHandlers;

public class NotFoundExceptionHandler: ExceptionHandlerBase<NotFoundException>
{
    protected override int GetStatusCode(NotFoundException ex) => StatusCodes.Status404NotFound;

    protected override string GetMessage(NotFoundException ex) => ex.Message;
}
