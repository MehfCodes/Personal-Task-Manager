using System;

namespace PTM.Domain.Models;

public class ResetPassword
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
    public required string Token { get; set; }
    public DateTime Expires { get; set; }
}
