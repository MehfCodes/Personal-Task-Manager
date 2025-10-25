using System;

namespace PTM.Contracts.Response.Token;

public class TokenPair
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
}
