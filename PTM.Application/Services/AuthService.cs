using System;
using PTM.Application.Interfaces.Services;
using PTM.Contracts.Requests;
using PTM.Contracts.Response;

namespace PTM.Application.Services;

public class AuthService : IAuthService
{


    public Task<UserResponse> Register(UserRegisterRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<UserResponse> Login(UserLoginRequest request)
    {
        throw new NotImplementedException();
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
