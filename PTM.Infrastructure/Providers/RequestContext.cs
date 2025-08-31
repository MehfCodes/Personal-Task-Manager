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
    public string? GetUserId() => httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    public string? GetUserAgent() => httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();
    public string? GetIpAddress() => httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            
}
