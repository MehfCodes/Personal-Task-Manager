using System;
using PTM.API.ExceptionHandlers;
using PTM.Application.Interfaces.Exceptions;

namespace PTM.API;

public static class DependencyInjection
{
    public static IServiceCollection AddExceptionHandlers(this IServiceCollection services)
    {
        services.AddSingleton<IExceptionHandler, ValidationExceptionHandler>();
        services.AddSingleton<IExceptionHandler, NotFoundExceptionHandler>();
        services.AddSingleton<IExceptionHandler, BusinessRuleExceptionHandler>();
        services.AddSingleton<IExceptionHandler, UnauthorizedExceptionHandler>();

        services.AddSingleton<IExceptionHandler, InternalExceptionHandler>();
        
        return services;
    }
}
