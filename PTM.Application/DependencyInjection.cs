using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PTM.Application.Interfaces.Services;
using PTM.Application.Mappers;
using PTM.Application.Services;
using PTM.Application.Validation;

namespace PTM.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IPlanService, PlanService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITaskItemService, TaskItemService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>(); 
        services.AddScoped<IUserPlanService, UserPlanService>(); 
        services.AddValidatorsFromAssemblyContaining<ValidationAssemblyMarker>();
        return services;
    }
}
