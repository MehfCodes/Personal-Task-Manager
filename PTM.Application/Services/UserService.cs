using System;
using PTM.Application.Exceptions;
using PTM.Application.Interfaces.Services;
using PTM.Application.Mappers;
using PTM.Contracts.Requests;
using PTM.Contracts.Response;
using PTM.Domain.Models;
using PTM.Infrastructure.Repository;

namespace PTM.Application.Services;

public class UserService : IUserService
{
    private readonly IBaseRepository<User> repository;

    public UserService(IBaseRepository<User> repository)
    {
        this.repository = repository;
    }
    public async Task<IEnumerable<UserResponse>> GetAllAsync() => (await repository.GetAllAsync()).MapToUsersResponse();

    public async Task<UserResponse> GetByIdAsync(Guid id)
    {
        var record = await repository.GetByIdAsync(id);
        if (record is null) throw new NotFoundException("User");
        return record.MapToUserResponse();
    }

    public async Task<UserResponse> UpdateAsync(Guid id, UserUpdateRequest request)
    {
        var record = await repository.GetByIdAsync(id);
        if (record is null) throw new NotFoundException("User");
        request.Id = record.Id;
        var updated = request.MapToUser(record);
        await repository.UpdateAsync(updated);
        return updated.MapToUserResponse();
    }
}
