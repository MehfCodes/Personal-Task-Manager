using System;

namespace PTM.Contracts.Requests;

public class UserUpdateRequest : BaseUserRequest
{
    public Guid Id { get; set; }
}
