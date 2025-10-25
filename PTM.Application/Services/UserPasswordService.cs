using Microsoft.Extensions.Logging;
using PTM.Application.Exceptions;
using PTM.Application.Interfaces;
using PTM.Application.Interfaces.Authentication;
using PTM.Application.Interfaces.Repositories;
using PTM.Application.Interfaces.Services;
using PTM.Contracts.Requests;
using PTM.Contracts.Requests.User;
using PTM.Contracts.Response.User;
using PTM.Domain.Models;

namespace PTM.Application.Services;

public class UserPasswordService : BaseService, IUserPasswordService
{
    private readonly IUserRepository userRepository;
    private readonly ITokenService tokenService;
    private readonly IResetPasswordRepository resetPasswordRepository;
    private readonly IRequestContext requestContext;
    private readonly IEmailService emailService;
    private readonly IRefreshTokenService refreshTokenService;
    private readonly IPasswordHasher passwordHasher;
    private readonly ILogger<UserPasswordService> logger;

    public UserPasswordService(IServiceProvider serviceProvider,
     IUserRepository userRepository,
     ITokenService tokenService,
     IResetPasswordRepository resetPasswordRepository,
     IRequestContext requestContext,
     IEmailService emailService,
     IRefreshTokenService refreshTokenService,
     IPasswordHasher passwordHasher,
     ILogger<UserPasswordService> logger) : base(serviceProvider)
    {
        this.userRepository = userRepository;
        this.tokenService = tokenService;
        this.resetPasswordRepository = resetPasswordRepository;
        this.requestContext = requestContext;
        this.emailService = emailService;
        this.refreshTokenService = refreshTokenService;
        this.passwordHasher = passwordHasher;
        this.logger = logger;
    }

    public async Task<ForgotPasswordResponse> ForgotPassword(ForgotPasswordRequest request)
    {
        await ValidateAsync(request);
        var user = await userRepository.GetUserByEmail(request.Email);
        if (user == null) throw new NotFoundException("User");
        var resetPasswordToken = Guid.NewGuid().ToString("N");
        var resetTokenHash = tokenService.HashToken(resetPasswordToken);
        var resetPassword = new ResetPassword
        {
            UserId = user.Id,
            Token = resetTokenHash,
            Expires = DateTime.UtcNow.AddMinutes(5)
        };
        await resetPasswordRepository.AddAsync(resetPassword);
        var emailBody = $"please click on the link below to reset your password: \n {requestContext.BuildResetPasswordLink(user.Email, resetPasswordToken)}";
        await emailService.SendEmailAsync(user.Email, "Reset Password", emailBody);
        logger.LogInformation("Reset Password Email Sent To User {UserId} at {Time}", user.Id, DateTime.UtcNow);
        return new ForgotPasswordResponse { Massage = "Check your email inbox, reset password link sent." };
    }
    public async Task<ResetPasswordResponse> ResetPassword(ResetPasswordRequest request)
    {
        await ValidateAsync(request);
        var hashedToken = tokenService.HashToken(request.Token);
        var user = await userRepository.GetUserByEmail(request.Email);
        if (user is null) throw new NotFoundException("User");
        var resetPassword = await resetPasswordRepository.GetResetPasswordByToken(hashedToken, user.Id);
        if (resetPassword is null) throw new UnauthorizedException("The request could not be authorized.");
        logger.LogWarning("Failed password reset attempt for user {UserId} at {Time}", user.Id, DateTime.UtcNow);
        user.Password = passwordHasher.HashPassword(request.NewPassword);
        user.PasswordChangedAt = DateTime.UtcNow;
        await userRepository.UpdateAsync(user);
        resetPassword.Expires = DateTime.UtcNow;
        await resetPasswordRepository.UpdateAsync(resetPassword);
        await refreshTokenService.RevokePreviousToken(user.Id, true);
        logger.LogInformation("Password reset successfully for user {UserId} at {Time}", user.Id, DateTime.UtcNow);
        return new ResetPasswordResponse { Massage = "password updated! please login." };
    }
     public async Task<UpdatePasswordResponse> UpdatePassword(UpdatePasswordRequest request)
    {
        await ValidateAsync(request);
        var userIdReq = requestContext.GetUserId();
        if (!userIdReq.HasValue) throw new UnauthorizedException();
        var user = await userRepository.GetByIdAsync(userIdReq.Value);
        if (user is null) throw new NotFoundException("User");
        if (!passwordHasher.VerifyPassword(request.CurrentPassword!, user.Password)) throw new UnauthorizedAccessException("The current password you entered is incorrect.");
        user.Password = passwordHasher.HashPassword(request.NewPassword!);
        user.PasswordChangedAt = DateTime.UtcNow;
        await userRepository.UpdateAsync(user);
        await refreshTokenService.RevokePreviousToken(user.Id, true);
        logger.LogInformation("User {UserId} Updated Password at {Time}", user.Id, DateTime.UtcNow);
        return new UpdatePasswordResponse { Massage = "password updated! please login." };
    }
}
