using System;

namespace PTM.Domain.Models;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; }= string.Empty;
    public string PhoneNumber { get; set; }= string.Empty;
    public string Password { get; set; }= string.Empty;
    public ICollection<UserPlan> UserPlans { get; set; } = [];
    public ICollection<TaskItem> Tasks { get; set; } = [];
}
