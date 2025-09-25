using System;
using BCrypt.Net;
using Microsoft.Extensions.Logging;
using PTM.Application.Exceptions;
using PTM.Application.Interfaces;
using PTM.Application.Interfaces.Authentication;
using PTM.Application.Interfaces.Providers;
using PTM.Application.Interfaces.Repositories;
using PTM.Application.Interfaces.Services;
using PTM.Application.Mappers;
using PTM.Contracts.Requests;
using PTM.Contracts.Requests.User;
using PTM.Contracts.Response;
using PTM.Contracts.Response.User;
using PTM.Domain.Models;
using PTM.Infrastructure.Repository;

namespace PTM.Application.Services;

public class AuthService : BaseService, IAuthService
{
    private readonly IUserRepository repository;
    private readonly ITokenGenerator tokenGenerator;
    private readonly IRefreshTokenService refreshTokenService;
    private readonly IRequestContext requestContext;
    private readonly IResetPasswordRepository resetPasswordRepository;
    private readonly ISmtpEmailSender smtpEmailSender;
    private readonly ILogger<AuthService> logger;
    private Guid? userIdReq;
    public AuthService(IUserRepository repository,
    ITokenGenerator tokenGenerator,
    IRefreshTokenService refreshTokenService,
    IRequestContext requestContext,
    IResetPasswordRepository resetPasswordRepository,
    ISmtpEmailSender smtpEmailSender,
    ILogger<AuthService> logger,
    IServiceProvider serviceProvider) : base(serviceProvider)
    {
        this.repository = repository;
        this.tokenGenerator = tokenGenerator;
        this.refreshTokenService = refreshTokenService;
        this.requestContext = requestContext;
        this.resetPasswordRepository = resetPasswordRepository;
        this.smtpEmailSender = smtpEmailSender;
        this.logger = logger;
        userIdReq = requestContext.GetUserId();
    }

    public async Task<UserResponse> Register(UserRegisterRequest request)
    {
        await ValidateAsync(request);
        var newUser = request.MapToUser();
        var record = await repository.AddAsync(newUser);
        var res = record.MapToUserResponse();
        var (raw, rt) = await refreshTokenService.CreateRefreshTokenAsync(newUser);
        res.AccessToken = tokenGenerator.CreateAccessToken(newUser, rt.Jti);
        res.RefreshToken = raw;
        logger.LogInformation("User {UserId} Registered at {Time}", res.Id, DateTime.UtcNow);
        return res;
    }

    public async Task<UserResponse> Login(UserLoginRequest request)
    {
        await ValidateAsync(request);
        var user = await repository.GetUserByEmail(request.Email!);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password)) throw new UnauthorizedException("Invalid username or password.");
        var res = user.MapToUserResponse();
        await refreshTokenService.RevokePreviousToken(user.Id);
        var (raw, rt) = await refreshTokenService.CreateRefreshTokenAsync(user);
        res.AccessToken = tokenGenerator.CreateAccessToken(user, rt.Jti);
        res.RefreshToken = raw;
        logger.LogInformation("User {UserId} logged in at {Time}", res.Id, DateTime.UtcNow);
        return res;
    }

    public async Task<RefreshTokenResponse> RefreshToken(RefreshTokenRequest request)
    {
        await ValidateAsync(request);
        var rt = await refreshTokenService.GenerateAndRevokeRefreshTokenAsync(request.RefreshToken);
        if (rt is null) throw new NotFoundException("Your session has expired.");
        return new RefreshTokenResponse { AccessToken = rt.AccessToken, RefreshToken = rt.RefreshToken };
    }

    public async Task<UpdatePasswordResponse> UpdatePassword(UpdatePasswordRequest request)
    {
        await ValidateAsync(request);
        if (!userIdReq.HasValue) throw new UnauthorizedException();
        var user = await repository.GetByIdAsync(userIdReq.Value);
        if (user is null) throw new NotFoundException("User");
        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.Password)) throw new UnauthorizedAccessException("The current password you entered is incorrect.");
        user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.PasswordChangedAt = DateTime.UtcNow;
        await repository.UpdateAsync(user);
        await refreshTokenService.RevokePreviousToken(user.Id, true);
        logger.LogInformation("User {UserId} Updated Password at {Time}", user.Id, DateTime.UtcNow);
        return new UpdatePasswordResponse { Massage = "password updated! please login." };
    }
    public async Task<LogoutResponse> Logout()
    {
        if (!userIdReq.HasValue) throw new UnauthorizedException();
        await refreshTokenService.RevokePreviousToken(userIdReq.Value);
        logger.LogInformation("User {UserId} logged out at {Time}", userIdReq.Value, DateTime.UtcNow);
        return new LogoutResponse { Massage = "you logout." };
    }
    public async Task<ForgotPasswordResponse> ForgotPassword(ForgotPasswordRequest request)
    {
        await ValidateAsync(request);
        var user = await repository.GetUserByEmail(request.Email);
        if (user == null) throw new NotFoundException("User");
        var resetPasswordToken = Guid.NewGuid().ToString("N");
        var resetTokenHash = tokenGenerator.HashToken(resetPasswordToken);
        var resetPassword = new ResetPassword
        {
            UserId = user.Id,
            Token = resetTokenHash,
            Expires = DateTime.UtcNow.AddMinutes(5)
        };
        await resetPasswordRepository.AddAsync(resetPassword);
        var emailBody = $"please click on the link below to reset your password \b {requestContext.BuildResetPasswordLink(user.Email, resetPasswordToken)}";
        await smtpEmailSender.SendEmailAsync(user.Email, "Reset Password", emailBody);
        logger.LogInformation("Reset Password Email Sent To User {UserId} at {Time}", user.Id, DateTime.UtcNow);
        return new ForgotPasswordResponse { Massage = "Check your email inbox, reset password link sent." };
    }
    public async Task<ResetPasswordResponse> ResetPassword(ResetPasswordRequest request)
    {
        await ValidateAsync(request);
        var hashedToken = tokenGenerator.HashToken(request.Token);
        var user = await repository.GetUserByEmail(request.Email);
        if (user is null) throw new NotFoundException("User");
        var resetPassword = await resetPasswordRepository.GetResetPasswordByToken(hashedToken, user.Id);
        if (resetPassword is null) throw new UnauthorizedException("The request could not be authorized.");
        logger.LogWarning("Failed password reset attempt for user {UserId} at {Time}", user.Id, DateTime.UtcNow);
        user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.PasswordChangedAt = DateTime.UtcNow;
        await repository.UpdateAsync(user);
        resetPassword.Expires = DateTime.UtcNow; 
        await resetPasswordRepository.UpdateAsync(resetPassword);
        await refreshTokenService.RevokePreviousToken(user.Id, true);
        logger.LogInformation("Password reset successfully for user {UserId} at {Time}", user.Id, DateTime.UtcNow);
        return new ResetPasswordResponse { Massage = "password updated! please login." };

    }
}
