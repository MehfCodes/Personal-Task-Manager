using System;
using PTM.API.ExceptionHandlers;
using PTM.Application.Interfaces.Exceptions;

namespace PTM.API;

public static class DependencyInjection
{
    public static IServiceCollection AddExceptionHandlers(this IServiceCollection services)
    {
        services.AddScoped<IExceptionHandler, ValidationExceptionHandler>();
        services.AddScoped<IExceptionHandler, NotFoundExceptionHandler>();
        services.AddScoped<IExceptionHandler, BusinessRuleExceptionHandler>();
        services.AddScoped<IExceptionHandler, UnauthorizedExceptionHandler>();

        services.AddSingleton<IExceptionHandler, InternalExceptionHandler>();
        
        return services;
    }
}
