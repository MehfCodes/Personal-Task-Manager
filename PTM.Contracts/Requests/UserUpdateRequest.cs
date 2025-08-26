using System;

namespace PTM.Contracts.Requests;

public class UserUpdateRequest
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; }= string.Empty;
    public string PhoneNumber { get; set; }= string.Empty;
}
