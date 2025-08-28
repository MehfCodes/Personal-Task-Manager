using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PTM.Application.Interfaces.Repositories;
using PTM.Infrastructure.Authentication;
using PTM.Infrastructure.Database;
using PTM.Infrastructure.Repository;

namespace PTM.Infrastructure;

public static class DependencyInjection 
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // connect to sql server
        var connectionString = config.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

        // add scoped
        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        services.AddScoped<IPlanRepository, PlanRepository>();

        // add jwt options
        var JwtOptions = config.GetSection("Jwt");
        services.Configure<JwtOptions>(JwtOptions);

        
        return services;
    }
}
