using System;
using PTM.Contracts.Requests;
using PTM.Contracts.Response;
using PTM.Domain.Models;

namespace PTM.Application.Mappers;

public static class UserContractMapper
{
    public static User MapToUser(this UserRequest request)
    {
        return new User
        {
            Username = request.Username,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Password = request.Password,
        };
    }
    public static UserResponse MapToUserResponse(this User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Password = user.Password,
            Plans = user.UserPlans.Select(up => new PlanResponse{ Id = up.PlanId, Title = up.Plan!.Title.ToString() }).ToList(),
        };
    }
}
