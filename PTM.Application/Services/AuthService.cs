using System;
using BCrypt.Net;
using PTM.Application.Interfaces;
using PTM.Application.Interfaces.Authentication;
using PTM.Application.Interfaces.Services;
using PTM.Application.Mappers;
using PTM.Contracts.Requests;
using PTM.Contracts.Response;
using PTM.Contracts.Response.User;
using PTM.Domain.Models;
using PTM.Infrastructure.Repository;

namespace PTM.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository repository;
    private readonly ITokenGenerator tokenGenerator;
    private readonly IRefreshTokenService refreshTokenService;
    private Guid? userIdReq;
    public AuthService(IUserRepository repository,
    ITokenGenerator tokenGenerator,
    IRefreshTokenService refreshTokenService,
    IRequestContext requestContext)
    {
        this.repository = repository;
        this.tokenGenerator = tokenGenerator;
        this.refreshTokenService = refreshTokenService;
        userIdReq = requestContext.GetUserId();
    }

    public async Task<UserResponse> Register(UserRegisterRequest request)
    {
        var newUser = request.MapToUser();
        var record = await repository.AddAsync(newUser);
        var res = record.MapToUserResponse();
        var (raw, rt) = await refreshTokenService.CreateRefreshTokenAsync(newUser);
        res.AccessToken = tokenGenerator.CreateAccessToken(newUser, rt.Jti);
        res.RefreshToken = raw;
        return res;
    }

    public async Task<UserResponse?> Login(UserLoginRequest request)
    {
        var user = await repository.GetUserByEmail(request.Email!);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password)) return null;
        var res = user.MapToUserResponse();
        await refreshTokenService.RevokePreviousToken(user.Id);
        var (raw, rt) = await refreshTokenService.CreateRefreshTokenAsync(user);
        res.AccessToken = tokenGenerator.CreateAccessToken(user, rt.Jti);
        res.RefreshToken = raw;
        return res;
    }

    public async Task<RefreshTokenResponse?> RefreshToken(string refreshToken)
    {
        var revoked = await refreshTokenService.GenerateAndRevokeRefreshTokenAsync(refreshToken);
        if (revoked is null) return null;
        return new RefreshTokenResponse { AccessToken = revoked.AccessToken, RefreshToken = revoked.RefreshToken };
    }

    public async Task<UpdatePasswordResponse?> UpdatePassword(UpdatePasswordRequest request)
    {
        if (!userIdReq.HasValue) return null; //please login
        var user = await repository.GetByIdAsync(userIdReq.Value);
        if (user is null) return null; // user not found
        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.Password)) return null; // current password is wrong
        user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.PasswordChangedAt = DateTime.UtcNow;
        await repository.UpdateAsync(user);
        await refreshTokenService.RevokePreviousToken(user.Id, true);
        return new UpdatePasswordResponse { Massage = "password updated! please login." };
    }
    public Task<ForgotPasswordResponse> ForgotPassword(ForgotPasswordRequest request)
    {
        throw new NotImplementedException();
    }
    public async Task<LogoutResponse?> Logout()
    {
        if (!userIdReq.HasValue) return null; //you are not login
        await refreshTokenService.RevokePreviousToken(userIdReq.Value);
        return new LogoutResponse { Massage = "you logout." };
    }
}
