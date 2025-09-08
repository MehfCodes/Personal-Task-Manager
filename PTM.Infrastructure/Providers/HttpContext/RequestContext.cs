using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PTM.Application.Interfaces.Services;

namespace PTM.Application.Services;

public class RequestContext : IRequestContext
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public RequestContext(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }
    public Guid? GetUserId()
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userId != null ? Guid.Parse(userId) : null;
    }

    public string? GetUserAgent() => httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();
    public string? GetIpAddress() => httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
    public string BuildResetPasswordLink(string email, string token)
    {
        var request = httpContextAccessor.HttpContext?.Request;
        var scheme = request?.Scheme;
        var host = request?.Host.Value;

        return $"{scheme}://{host}/api/auth/reset-password?email={email}&token={token}";
    }
}
