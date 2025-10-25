using System;

namespace PTM.Contracts.Response.Token;

public class RevokeResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}