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
    private readonly IRefreshTokenService refreshTokenService;

    public AuthService(IUserRepository repository,
    ITokenGenerator tokenGenerator,
    IRefreshTokenService refreshTokenService)
{
        this.repository = repository;
        this.tokenGenerator = tokenGenerator;
        this.refreshTokenService = refreshTokenService;
    }

    public async Task<UserResponse> Register(UserRegisterRequest request, string ipAddress, string userAgent)
    {
        var newUser = request.MapToUser();
        var record = await repository.AddAsync(newUser);
        var res = record.MapToUserResponse();
        res.AccessToken = tokenGenerator.CreateAccessToken(newUser);
        var (raw, _) = await refreshTokenService.CreateRefreshTokenAsync(newUser, ipAddress, userAgent);
        res.RefreshToken = raw;
        return res;
    }

    public async Task<UserResponse?> Login(UserLoginRequest request, string ipAddress, string userAgent)
    {
        var user = await repository.GetUserByEmail(request.Email!);
        if (user is null || BCrypt.Net.BCrypt.Verify(request.Password, user.Password)) return null;
        var res = user.MapToUserResponse();
        res.AccessToken = tokenGenerator.CreateAccessToken(user);
        var (raw, _) = await refreshTokenService.CreateRefreshTokenAsync(user, ipAddress, userAgent);
        res.RefreshToken = raw;
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
