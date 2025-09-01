using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PTM.Application.Interfaces;
using PTM.Infrastructure.Authentication;

namespace PTM.API.Middlewares;

public class ProtectedRoute
{
    private readonly string jwtSecretKey;
    private readonly RequestDelegate next;
    private readonly IUserRepository userRepository;

    public ProtectedRoute(RequestDelegate next, IOptions<JwtOptions> options, IUserRepository userRepository)
    {
        this.next = next;
        jwtSecretKey = options.Value.SecretKey;
        this.userRepository = userRepository;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        var token = httpContext.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();
        if (string.IsNullOrEmpty(token))
        {
            httpContext.Response.StatusCode = 401;
            await httpContext.Response.WriteAsync("Token is missing");
            return;
        }
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(jwtSecretKey);
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                httpContext.Response.StatusCode = 401;
                await httpContext.Response.WriteAsync("Invalid token");
                return;
            }
            var user = await userRepository.GetByIdAsync(Guid.Parse(userIdClaim));
            if (user == null)
            {
                httpContext.Response.StatusCode = 401;
                await httpContext.Response.WriteAsync("User not found");
                return;
            }

            if (user.PasswordChangedAt > jwtToken.IssuedAt)
            {
                httpContext.Response.StatusCode = 401;
                await httpContext.Response.WriteAsync("Password changed. Please login again.");
                return;
            }
            await next(httpContext);
        }
        catch (Exception)
        {
            httpContext.Response.StatusCode = 401;
            await httpContext.Response.WriteAsync("Invalid token");
        }

    }
}
