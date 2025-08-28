using System;

namespace PTM.Infrastructure.Authentication;

public class JwtOptions
{
    public string SecretKey { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int AccessTokenDuration { get; init; }
    public int RefreshTokenDuration { get; init; }
    public char[] SigningKey { get; internal set; }
}
