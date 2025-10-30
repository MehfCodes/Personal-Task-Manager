using System;
using PTM.Contracts.Requests;
using PTM.Contracts.Response;

namespace PTM.Application.Interfaces.Services;

public interface IUserService
{
    Task<UserResponse> GetByIdAsync(Guid id);
    Task<IEnumerable<UserResponse>> GetAllAsync();
    Task<UserResponse> UpdateAsync(Guid id, UserUpdateRequest user);
    Task<MessageResponse> PromoteToAdminAsync(Guid id);
}
