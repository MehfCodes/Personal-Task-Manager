using Microsoft.Extensions.DependencyInjection;
using PTM.Application.Interfaces.Services;
using PTM.Application.Mappers;
using PTM.Application.Services;

namespace PTM.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IPlanService, PlanService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITaskItemService, TaskItemService>();
        return services;
    }
}
