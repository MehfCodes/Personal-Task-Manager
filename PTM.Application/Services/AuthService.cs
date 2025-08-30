using System;
using PTM.Application.Interfaces;
using PTM.Application.Interfaces.Authentication;
using PTM.Application.Interfaces.Services;
using PTM.Application.Mappers;
using PTM.Contracts.Requests;
using PTM.Contracts.Response;
using PTM.Domain.Models;
using PTM.Infrastructure.Repository;

namespace PTM.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository repository;
    private readonly ITokenGenerator tokenGenerator;

    public AuthService(IUserRepository repository, ITokenGenerator tokenGenerator)
{
        this.repository = repository;
        this.tokenGenerator = tokenGenerator;
    }

    public async Task<UserResponse> Register(UserRegisterRequest request)
    {
        var newUser = request.MapToUser();
        var record = await repository.AddAsync(newUser);
        var res = record.MapToUserResponse();
        res.AccessToken = tokenGenerator.CreateAccessToken(newUser);
        res.RefreshToken = tokenGenerator.CreateRefreshToken().tokenHash;
        return res;
    }

    public async Task<UserResponse> Login(UserLoginRequest request)
    {
        var user = await repository.GetUserByEmail(request.Email!);
        if (user is null || BCrypt.Net.BCrypt.Verify(request.Password, user.Password)) throw new UnauthorizedAccessException();
        var res = user.MapToUserResponse();
        res.AccessToken = tokenGenerator.CreateAccessToken(user);
        res.RefreshToken = tokenGenerator.CreateRefreshToken().tokenHash;
        return res;
    }

    public Task<RefreshTokenResponse> RefreshToken(RefreshTokenRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<UpdatePasswordResponse> UpdatePassword(UpdatePasswordRequest request)
    {
        throw new NotImplementedException();
    }
    public Task<ForgotPasswordResponse> ForgotPassword(ForgotPasswordRequest request)
    {
        throw new NotImplementedException();
    }
}
