using System;
using System.Reflection;
using Microsoft.OpenApi.Models;
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
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "PTM API",
                Version = "v1",
                Description = "PTM — API documentation"
            });

            // XML comments
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);

           
            c.EnableAnnotations();

            // JWT Bearer (Authorize button)
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter: Bearer {your_jwt_token}"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    new string[] { }
                }
            });
        });
        
        return services;
    }
}
