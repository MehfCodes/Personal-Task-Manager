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
    private readonly IRequestContext requestContext;
    private readonly string ipAddress;
    private readonly string userAgent;


    public AuthService(IUserRepository repository,
    ITokenGenerator tokenGenerator,
    IRefreshTokenService refreshTokenService,
    IRequestContext requestContext)
    {
        this.repository = repository;
        this.tokenGenerator = tokenGenerator;
        this.refreshTokenService = refreshTokenService;
        ipAddress = requestContext.GetIpAddress() ?? "";
        userAgent = requestContext.GetUserAgent() ?? "";

    }

    public async Task<UserResponse> Register(UserRegisterRequest request)
    {
        var newUser = request.MapToUser();
        var record = await repository.AddAsync(newUser);
        var res = record.MapToUserResponse();
        res.AccessToken = tokenGenerator.CreateAccessToken(newUser);
        var (raw, _) = await refreshTokenService.CreateRefreshTokenAsync(newUser, ipAddress, userAgent);
        res.RefreshToken = raw;
        return res;
    }

    public async Task<UserResponse?> Login(UserLoginRequest request)
    {
        var user = await repository.GetUserByEmail(request.Email!);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password)) return null;
        var res = user.MapToUserResponse();
        res.AccessToken = tokenGenerator.CreateAccessToken(user);
        var (raw, _) = await refreshTokenService.CreateRefreshTokenAsync(user, ipAddress, userAgent);
        res.RefreshToken = raw;
        return res;
    }

    public async Task<RefreshTokenResponse?> RefreshToken(RefreshTokenRequest request)
    {
        var rt = await refreshTokenService.GetRefreshToken(request.RefreshToken!);
        if (rt is null) return null;
        var user = await repository.GetByIdAsync(rt.UserId);
        if (user is null) return null;
        var revoked= await refreshTokenService.RevokeRefreshTokenAsync(request.RefreshToken!, user, ipAddress, userAgent);
        if (revoked is null) return null;
        return new RefreshTokenResponse { AccessToken = revoked.AccessToken, RefreshToken = revoked.RefreshToken };
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
