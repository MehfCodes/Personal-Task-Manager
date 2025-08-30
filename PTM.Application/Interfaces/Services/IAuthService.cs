using System;
using PTM.Contracts.Requests;
using PTM.Contracts.Response;

namespace PTM.Application.Interfaces.Services;

public interface IAuthService
{
    Task<UserRegisterResponse> Register(UserRegisterRequest request);
    Task<UserLoginResponse> Login(UserLoginRequest request); 
    Task<RefreshTokenResponse> RefreshToken(RefreshTokenRequest request); 
    Task<ForgotPasswordResponse> ForgotPassword(ForgotPasswordRequest request); 
    Task<UpdatePasswordResponse> UpdatePassword(UpdatePasswordRequest request); 
}
