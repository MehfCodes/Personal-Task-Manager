using System;

namespace PTM.Contracts.Requests;

public class RefreshTokenResponse
{
    public string RefreshToken { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
}
