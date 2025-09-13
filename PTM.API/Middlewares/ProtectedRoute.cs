using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PTM.Application.Exceptions;
using PTM.Application.Interfaces;
using PTM.Application.Interfaces.Repositories;
using PTM.Application.Interfaces.Services;
using PTM.Infrastructure.Authentication;

namespace PTM.API.Middlewares;

public class ProtectedRoute
{
    private readonly string jwtSecretKey;
    private readonly RequestDelegate next;
    private readonly JwtOptions JwtOpt;

    public ProtectedRoute(RequestDelegate next, IOptions<JwtOptions> options)
    {
        this.next = next;
        jwtSecretKey = options.Value.SecretKey;
        JwtOpt = options.Value;
    }

    public async Task InvokeAsync(HttpContext httpContext, IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, IRequestContext requestContext)
    {
        var ipAddress = requestContext.GetIpAddress() ?? "";
        var userAgent = requestContext.GetUserAgent() ?? "";

        var endpoint = httpContext.GetEndpoint();
        if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() is object)
        {
            await next(httpContext);
            return;
        }
        var token = httpContext.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();
        if (string.IsNullOrEmpty(token))
        {
            throw new UnauthorizedException("Token is missing");
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
                ValidIssuer = JwtOpt.Issuer,
                ValidAudience = JwtOpt.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedException("Invalid Token");
            }
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
            await next(httpContext);
        }
        catch (SecurityTokenExpiredException)
        {
            throw new UnauthorizedException("Your session has expired. Please login again.");
        }
        catch (SecurityTokenValidationException)
        {
            throw new UnauthorizedException("Invalid authentication information.");
        }
        
    }
       

    
}
