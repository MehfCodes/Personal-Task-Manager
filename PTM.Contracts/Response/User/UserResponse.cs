using System;

namespace PTM.Contracts.Response;

public class UserPlanRes
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    
}
public class UserResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public IEnumerable<UserPlanRes> Plans { get; set; } = [];
    public string RefreshToken { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;

}
