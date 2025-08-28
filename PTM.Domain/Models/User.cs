namespace PTM.Domain.Models;

public enum UserRole { User, Admin }
public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public ICollection<UserPlan> UserPlans { get; set; } = [];
    public ICollection<TaskItem> Tasks { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
