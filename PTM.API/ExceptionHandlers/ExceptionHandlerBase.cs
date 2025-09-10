using System;
using PTM.Application.Exceptions;
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
    public IDictionary<string, string[]> GetErrors(TException ex)
    {
        if (ex is ValidationException validationEx)
        {
            return validationEx.Errors.GroupBy(e => e.PropertyName).ToDictionary(
            k => k.Key,
            v => v.Select(e => e.ErrorMessage).ToArray());
        }
        else
        {
            return new Dictionary<string, string[]>
        {
            { "Message", new string[] { ex.Message } }
        };
        }
    }
}
