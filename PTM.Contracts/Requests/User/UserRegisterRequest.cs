using System;

namespace PTM.Contracts.Requests;

public class UserRegisterRequest : BaseUserRequest
{
    public string Password { get; set; }= string.Empty;
}
