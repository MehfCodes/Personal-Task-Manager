using Microsoft.Extensions.DependencyInjection;
using PTM.Application.Services;

namespace PTM.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IPlanService, PlanService>();
        return services;
    }
}
