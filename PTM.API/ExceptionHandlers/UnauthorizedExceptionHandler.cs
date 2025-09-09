using System;
using PTM.Application.Exceptions;

namespace PTM.API.ExceptionHandlers;

public class UnauthorizedExceptionHandler : ExceptionHandlerBase<UnauthorizedException>
{
    protected override int GetStatusCode(UnauthorizedException ex) => StatusCodes.Status401Unauthorized;

    protected override string GetMessage(UnauthorizedException ex) => ex.Message;
}
