using System;

namespace PTM.Application.Interfaces.Services;

public interface IRequestContext
{
    string? GetUserId();
    string? GetUserAgent();
    string? GetIpAddress();
}