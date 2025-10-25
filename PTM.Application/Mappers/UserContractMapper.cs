using PTM.Contracts.Requests;
using PTM.Contracts.Response;
using PTM.Contracts.Response.UserPlan;
using PTM.Domain.Models;

namespace PTM.Application.Mappers;

public static class UserContractMapper
{
    public static User MapToUser(this UserRegisterRequest request)
    {
        return new User
        {
            Username = request.Username,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
        };
    }
    public static User MapToUser(this UserUpdateRequest request, User user)
    {
        user.Username = request.Username;
        user.Email = request.Email;
        user.PhoneNumber = request.PhoneNumber;
        return user;
    }
    public static UserResponse MapToUserResponse(this User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Plans = user.UserPlans.Select(up => new PlanResponse{ Id = up.PlanId, Title = up.Plan!.Title.ToString() }).ToList(),
        };
    }
    public static UserRes MapToUserForUserPlanResponse(this User user)
    {
        return new UserRes
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
        };
    }
    public static IEnumerable<UserResponse> MapToUsersResponse(this IEnumerable<User> users) => users.Select(MapToUserResponse);

}
