using System;
using System.Text.Json;
using PTM.API.ExceptionHandlers;
using PTM.Application.Interfaces.Exceptions;

namespace PTM.API.Middlewares;

public class ExceptionHandlingMiddleware{
    private readonly RequestDelegate next;
    private readonly ILogger<ExceptionHandlingMiddleware> logger;
    private readonly IEnumerable<IExceptionHandler> handlers;
    private readonly InternalExceptionHandler internalHandler = new InternalExceptionHandler();

    public ExceptionHandlingMiddleware(RequestDelegate next, IEnumerable<IExceptionHandler> handlers, ILogger<ExceptionHandlingMiddleware> logger)
    {
        this.next = next;
        this.handlers = handlers;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occurred while processing request {Path}", context.Request.Path);
            var handler = handlers.FirstOrDefault(h => h.CanHandle(ex)) ?? internalHandler;

            var response = handler.Handle(ex, context.TraceIdentifier);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = response.Status;

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}