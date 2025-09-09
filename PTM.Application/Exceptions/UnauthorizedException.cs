using System;

namespace PTM.Application.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string? message = null)
        : base(message ?? "Unauthorized, Access Denied.")
    {
    }
}