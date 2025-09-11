using System;
using PTM.Contracts.Requests;
using PTM.Contracts.Requests.User;
using PTM.Contracts.Response;
using PTM.Contracts.Response.User;

namespace PTM.Application.Interfaces.Services;

public interface IAuthService
{
    Task<UserResponse> Register(UserRegisterRequest request);
    Task<UserResponse> Login(UserLoginRequest request);
    Task<RefreshTokenResponse> RefreshToken(string refreshToken);
    Task<ForgotPasswordResponse> ForgotPassword(ForgotPasswordRequest request);
    Task<ResetPasswordResponse> ResetPassword(ResetPasswordRequest request);
    Task<UpdatePasswordResponse> UpdatePassword(UpdatePasswordRequest request);
    Task<LogoutResponse> Logout();
}
