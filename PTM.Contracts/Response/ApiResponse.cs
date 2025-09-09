using System;

namespace PTM.Contracts.Response;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public int Status { get; set; }
    public string? Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
    public string? TraceId { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string? message = null, string? traceId = null, int status = 200)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Status = status,
            Message = message,
            Data = data,
            TraceId = traceId
        };
    }

    public static ApiResponse<T> ErrorResponse(string? message = null, int status = 500, Dictionary<string, string[]>? errors = null, string? traceId = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Status = status,
            Message = message,
            Errors = errors,
            TraceId = traceId
        };
    }
}
