using System;
using PTM.Contracts.Requests;
using PTM.Contracts.Response;

namespace PTM.Application.Interfaces.Services;

public interface IAuthService
{
    Task<UserResponse> Register(UserRegisterRequest request, string ipAddress, string userAgent);
    Task<UserResponse?> Login(UserLoginRequest request, string ipAddress, string userAgent); 
    Task<RefreshTokenResponse> RefreshToken(RefreshTokenRequest request); 
    Task<ForgotPasswordResponse> ForgotPassword(ForgotPasswordRequest request); 
    Task<UpdatePasswordResponse> UpdatePassword(UpdatePasswordRequest request); 
}
