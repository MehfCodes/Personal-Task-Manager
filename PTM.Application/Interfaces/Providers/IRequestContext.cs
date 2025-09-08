
using System.Net.Http;

namespace PTM.Application.Interfaces.Services;

public interface IRequestContext
{
    Guid? GetUserId();
    string? GetUserAgent();
    string? GetIpAddress();
    string BuildResetPasswordLink(string email, string token);
}