using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PTM.Application.Interfaces;
using PTM.Application.Interfaces.Authentication;
using PTM.Application.Interfaces.Providers;
using PTM.Application.Interfaces.Repositories;
using PTM.Application.Interfaces.Services;
using PTM.Application.Services;
using PTM.Infrastructure.Authentication;
using PTM.Infrastructure.Database;
using PTM.Infrastructure.Providers;
using PTM.Infrastructure.Providers.Email;
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
        services.AddScoped<ITokenGenerator, TokenGenerator>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITaskItemRepository, TaskItemRepository>();
        services.AddHttpContextAccessor();
        services.AddScoped<IRequestContext, RequestContext>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IResetPasswordRepository, ResetPasswordRepository>();
        services.AddScoped<ISmtpEmailSender, SmtpEmailSender>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        var HashSecret = config.GetSection("HashSecret");
        services.Configure<HashSecret>(HashSecret);

        // Email Config: Smtp
        var smtpSettings = config.GetSection("SmtpSettings");
        services.Configure<SmtpSettings>(smtpSettings);

        // add jwt options
        var JwtOptions = config.GetSection("Jwt");
        services.Configure<JwtOptions>(JwtOptions);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = config["Jwt:Issuer"],
                ValidAudience = config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:SecretKey"]!)),
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
            options.AddPolicy("AdminOrUser", policy => policy.RequireRole("Admin","User"));
         
        });
        return services;
    }
}
