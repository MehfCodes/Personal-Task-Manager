using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PTM.Application.Exceptions;
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

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = async context =>
                {
                    if (context.Exception is SecurityTokenExpiredException)
                    {
                        throw new UnauthorizedException("Your session has expired. Please login again.");
                    }
                    else if (context.Exception is SecurityTokenValidationException)
                    {
                        throw new UnauthorizedException("Invalid authentication information.");
                    }
                    else if (context.Exception is SecurityTokenValidationException)
                    {
                        throw new UnauthorizedException("Invalid authentication information.");
                    }
                    return;
                },
                OnTokenValidated = async context =>
                {
                    var endpoint = context.HttpContext.GetEndpoint();
                    var adminOnly = endpoint?.Metadata?.GetMetadata<IAuthorizeData>()?.Roles?.Contains("Admin") ?? false;
                    var claims = context.Principal?.Claims;
                    var userIdClaim = claims?.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub || c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
                    var jti = claims?.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
                    var tokenRole = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
                    // if (tokenRole != "Admin" && adminOnly)
                    // {
                    //     throw new UnauthorizedException("Access Denied!");
                    // }
                    if (string.IsNullOrEmpty(userIdClaim))
                    {
                        throw new UnauthorizedException("Invalid token claims!");
                    }
                    var userRepository = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                    var refreshTokenRepository = context.HttpContext.RequestServices.GetRequiredService<IRefreshTokenRepository>();
                    var requestContext = context.HttpContext.RequestServices.GetRequiredService<IRequestContext>();
                    var ipAddress = requestContext.GetIpAddress() ?? "";
                    var userAgent = requestContext.GetUserAgent() ?? "";
                    var user = await userRepository.GetByIdAsync(Guid.Parse(userIdClaim));
                    if (user == null)
                    {
                        throw new UnauthorizedException("User not found");
                    }
                    var rt = await refreshTokenRepository.GetRefreshTokenByUserId(Guid.Parse(userIdClaim), ipAddress, userAgent);
                    if (rt is null || rt.Jti.ToString() != jti)
                    {
                        throw new UnauthorizedException("Session is no longer valid.");
                    }
                },
                OnForbidden = async context =>
                {
                    throw new UnauthorizedException("Forbiden, Access Denied!");
                }

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
