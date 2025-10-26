using Moq;
using PTM.Application.Interfaces;
using PTM.Application.Interfaces.Authentication;
using PTM.Application.Interfaces.Repositories;
using PTM.Application.Interfaces.Services;
using PTM.Application.Services;
using Microsoft.Extensions.Logging;
using PTM.Domain.Models;
using PTM.Contracts.Requests;
using FluentAssertions;
using PTM.Application.Exceptions;
using PTM.Contracts.Requests.User;

namespace PTM.UnitTests.Services;

public class UserPasswordServiceTests
{
    private readonly Mock<IUserRepository> userRepoMock = new();
    private readonly Mock<ITokenService> tokenServiceMock = new();
    private readonly Mock<IRefreshTokenService> refreshTokenServiceMock = new();
    private readonly Mock<IRequestContext> requestContextMock = new();
    private readonly Mock<IRequestContext> requestContextNullMock = new();
    private readonly Mock<IResetPasswordRepository> resetRepoMock = new();
    private readonly Mock<IEmailService> EmailServiceMock = new();
    private readonly Mock<IPasswordHasher> passwordHasherMock = new();
    private readonly Guid? userIdReq = Guid.NewGuid();
    private readonly Mock<IServiceProvider> serviceProviderMock = new();
    private readonly Mock<ILogger<UserPasswordService>> loggerMock = new();
    private readonly UserPasswordService userPasswordServiceMock;

    public UserPasswordServiceTests()
    {
        requestContextMock.Setup(repo => repo.GetUserId()).Returns(userIdReq);
        requestContextNullMock.Setup(repo => repo.GetUserId()).Returns((Guid?)null);
        userPasswordServiceMock = new UserPasswordService(
            serviceProviderMock.Object,
            userRepoMock.Object,
            tokenServiceMock.Object,
            resetRepoMock.Object,
            requestContextMock.Object,
            EmailServiceMock.Object,
            refreshTokenServiceMock.Object,
            passwordHasherMock.Object,
            loggerMock.Object
            );
    }

    
    [Fact]
    public async Task UpdatePassword_ShouldBeSuccessfull()
    {
        var userId = Guid.NewGuid();
        requestContextMock.Setup(repo => repo.GetUserId()).Returns(userId);
        var rawPassword = "123456@Test";
        passwordHasherMock.Setup(ph => ph.HashPassword(rawPassword)).Returns((string pw) => "hashed_" + pw);
        var user = new User
        {
            Id = userId,
            Email = "test@test.com",
            Password = "Hashed"
        };
        var req = new UpdatePasswordRequest
        {
            CurrentPassword = rawPassword,
            NewPassword = "abc"
        };
        passwordHasherMock.Setup(ph => ph.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        requestContextMock.Setup(rc => rc.GetUserId()).Returns(userId);
        userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        refreshTokenServiceMock.Setup(rts => rts.RevokePreviousToken(userId, true)).Returns(Task.CompletedTask);

        var result = await userPasswordServiceMock.UpdatePassword(req);
        result.Should().NotBeNull();
        result.Massage.Should().Be("password updated! please login.");

    }
    [Fact]
    public async Task UpdatePassword_ShouldThrowUnauthorizedException__WhenUserIdIsNull()
    {
        var rawPassword = "123456@Test";
        var req = new UpdatePasswordRequest
        {
            CurrentPassword = rawPassword,
            NewPassword = "abc"
        };
        requestContextMock.Setup(rc => rc.GetUserId()).Returns((Guid?)null);
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => userPasswordServiceMock.UpdatePassword(req));

        exception.Message.Should().Contain("Unauthorized");
        userRepoMock.Verify(r => r.GetByIdAsync(Guid.NewGuid()), Times.Never);
    }
    [Fact]
    public async Task UpdatePassword_ShouldThrowNotFoundException__WhenUserNotFound()
    {
        var userId = Guid.NewGuid();
        var rawPassword = "123456@Test";
        var req = new UpdatePasswordRequest
        {
            CurrentPassword = rawPassword,
            NewPassword = "abc"
        };
        requestContextMock.Setup(rc => rc.GetUserId()).Returns(userId);
        userRepoMock.Setup(r => r.GetByIdAsync(Guid.NewGuid())).ReturnsAsync((User?)null);
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => userPasswordServiceMock.UpdatePassword(req));

        exception.Message.Should().Contain("User not found");
        userRepoMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }
    [Fact]
    public async Task UpdatePassword_ShouldThrowNotUnauthorizedAccessException__WhenCurrentPasswordIsWrong()
    {
        var userId = Guid.NewGuid();
        requestContextMock.Setup(repo => repo.GetUserId()).Returns(userId);
        var rawPassword = "123456@Test";
        passwordHasherMock.Setup(ph => ph.HashPassword(rawPassword)).Returns((string pw) => "hashed_" + pw);
        var user = new User
        {
            Id = userId,
            Email = "test@test.com",
            Password = "hashedPassword"
        };
        var req = new UpdatePasswordRequest
        {
            CurrentPassword = "qwe",
            NewPassword = "abc"
        };
        requestContextMock.Setup(rc => rc.GetUserId()).Returns(userId);
        userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        passwordHasherMock.Setup(ph => ph.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => userPasswordServiceMock.UpdatePassword(req));

        exception.Message.Should().Contain("The current password you entered is incorrect.");
        userRepoMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task ForgotPassword__ShouldBeSuccessfull()
    {
        var userId = Guid.NewGuid();

        var req = new ForgotPasswordRequest
        {
            Email = "test@test.com",
        };

        var expectedUser = new User
        {
            Id = userId,
            Email = req.Email,
        };

        var resetPassword = new ResetPassword
        {
            UserId = userId,
            Token = "resetTokenHash"
        };
        userRepoMock.Setup(r => r.GetUserByEmail(req.Email)).ReturnsAsync(expectedUser);

        tokenServiceMock.Setup(t => t.HashToken("token")).Returns("access-token");
        resetRepoMock.Setup(r => r.AddAsync(It.IsAny<ResetPassword>())).ReturnsAsync(resetPassword);

        EmailServiceMock.Setup(r => r.SendEmailAsync(req.Email, "Reset Password", "email")).Returns(Task.CompletedTask);

        var result = await userPasswordServiceMock.ForgotPassword(req);
        result.Should().NotBeNull();
        result.Massage.Should().Be("Check your email inbox, reset password link sent.");
    }
    [Fact]
    public async Task ForgotPassword__ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        var req = new ForgotPasswordRequest
        {
            Email = "test@test.com",
        };

        userRepoMock.Setup(r => r.GetUserByEmail(req.Email)).ReturnsAsync((User?)null);
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => userPasswordServiceMock.ForgotPassword(req));
        exception.Message.Should().Contain("User not found");
        EmailServiceMock.Verify(r => r.SendEmailAsync(req.Email, "Reset Password", "email"), Times.Never);
    }
    [Fact]
    public async Task ResetPassword__ShouldBeSuccessfull()
    {
        var userId = Guid.NewGuid();

        var req = new ResetPasswordRequest
        {
            Token = "token",
            Email = "test@test.com",
            NewPassword = "123"
        };

        var expectedUser = new User
        {
            Id = userId,
            Email = req.Email,
        };

        var resetPassword = new ResetPassword
        {
            UserId = userId,
            Token = "resetTokenHash"
        };
        var hashedToken = "hashed-token";
        tokenServiceMock.Setup(t => t.HashToken(req.Token)).Returns(hashedToken);
        userRepoMock.Setup(r => r.GetUserByEmail(req.Email)).ReturnsAsync(expectedUser);

        resetRepoMock.Setup(r => r.GetResetPasswordByToken(hashedToken, userId)).ReturnsAsync(resetPassword);
        userRepoMock.Setup(r => r.UpdateAsync(expectedUser)).Returns(Task.CompletedTask);
        resetRepoMock.Setup(r => r.UpdateAsync(resetPassword)).Returns(Task.CompletedTask);
        refreshTokenServiceMock.Setup(rts => rts.RevokePreviousToken(userId, true)).Returns(Task.CompletedTask);

        var result = await userPasswordServiceMock.ResetPassword(req);
        result.Should().NotBeNull();
        result.Massage.Should().Be("password updated! please login.");

    }
    [Fact]
    public async Task ResetPassword__ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();

        var req = new ResetPasswordRequest
        {
            Token = "token",
            Email = "test@test.com",
            NewPassword = "123"
        };

        var hashedToken = "hashed-token";
        tokenServiceMock.Setup(t => t.HashToken(req.Token)).Returns(hashedToken);
        userRepoMock.Setup(r => r.GetUserByEmail(req.Email)).ReturnsAsync((User?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => userPasswordServiceMock.ResetPassword(req));
        exception.Message.Should().Contain("User not found");
    }
    [Fact]
    public async Task ResetPassword__ShouldThrowUnauthorizedException__WhenUserIdIsNull()
    {
        var userId = Guid.NewGuid();
       
        var req = new ResetPasswordRequest
        {
            Token = "token",
            Email = "test@test.com",
            NewPassword="123"
        };

        var expectedUser = new User
        {
            Id = userId,
            Email = req.Email,
        };

        var resetPassword = new ResetPassword
        {
            UserId = userId,
            Token = "resetTokenHash"
        };
        var hashedToken = "hashed-token";
        tokenServiceMock.Setup(t => t.HashToken(req.Token)).Returns(hashedToken);
        userRepoMock.Setup(r => r.GetUserByEmail(req.Email)).ReturnsAsync(expectedUser);

        resetRepoMock.Setup(r => r.GetResetPasswordByToken(hashedToken, userId)).ReturnsAsync((ResetPassword?)null);
       
       var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => userPasswordServiceMock.ResetPassword(req));

        exception.Message.Should().Contain("The request could not be authorized.");
        userRepoMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }
}
