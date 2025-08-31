using System;

namespace PTM.Application.Interfaces.Services;

public interface IRequestContext
{
    Guid? GetUserId();
    string? GetUserAgent();
    string? GetIpAddress();
}