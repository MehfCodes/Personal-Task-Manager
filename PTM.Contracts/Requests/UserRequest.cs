using System;

namespace PTM.Contracts.Requests;

public class UserRequest : BaseUserRequest
{
    public string Password { get; set; }= string.Empty;
}
