using System;

namespace PTM.Contracts.Requests;

public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}
