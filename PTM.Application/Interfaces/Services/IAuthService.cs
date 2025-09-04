using System;
using PTM.Contracts.Requests;
using PTM.Contracts.Response;

namespace PTM.Application.Interfaces.Services;

public interface IAuthService
{
    Task<UserResponse> Register(UserRegisterRequest request);
    Task<UserResponse?> Login(UserLoginRequest request); 
    Task<RefreshTokenResponse?> RefreshToken(string refreshToken); 
    Task<ForgotPasswordResponse> ForgotPassword(ForgotPasswordRequest request); 
    Task<UpdatePasswordResponse?> UpdatePassword(UpdatePasswordRequest request); 
}
