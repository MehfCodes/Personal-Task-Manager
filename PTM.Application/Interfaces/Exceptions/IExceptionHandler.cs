using System;
using PTM.Contracts.Response;

namespace PTM.Application.Interfaces.Exceptions;

public interface IExceptionHandler
{
    bool CanHandle(Exception ex);
    ApiResponse<object> Handle(Exception ex, string traceId);
}
