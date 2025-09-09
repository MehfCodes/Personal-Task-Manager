using System;
using PTM.Application.Exceptions;

namespace PTM.API.ExceptionHandlers;

public class BusinessRuleExceptionHandler : ExceptionHandlerBase<BusinessRuleException>
{
    protected override int GetStatusCode(BusinessRuleException ex) => StatusCodes.Status422UnprocessableEntity;

    protected override string GetMessage(BusinessRuleException ex) => ex.Message;
}

