using System;

namespace PTM.Contracts.Requests;

public class UserReRequest : BaseUserRequest
{
    public string Password { get; set; }= string.Empty;
}
