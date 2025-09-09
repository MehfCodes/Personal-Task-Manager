using System;
using PTM.Application.Interfaces.Exceptions;
using PTM.Contracts.Response;

namespace PTM.API.ExceptionHandlers;

public abstract class ExceptionHandlerBase<TException> : IExceptionHandler where TException : Exception
{
    public bool CanHandle(Exception ex) => ex is TException;

    public ApiResponse<object> Handle(Exception ex, string traceId)
    {
        var typedEx = (TException)ex;

        return new ApiResponse<object>
        {
            Success = false,
            Status = GetStatusCode(typedEx),
            Message = GetMessage(typedEx),
            Errors = GetErrors(typedEx),
            TraceId = traceId
        };
    }
    
    protected abstract int GetStatusCode(TException ex);
    protected abstract string GetMessage(TException ex);
    protected virtual IDictionary<string, string[]>? GetErrors(TException ex) => null;
}
