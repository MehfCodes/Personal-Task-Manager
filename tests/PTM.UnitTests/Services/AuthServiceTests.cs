
using FluentAssertions;
using Moq;
using PTM.Application.Exceptions;
using PTM.Application.Interfaces;
using PTM.Application.Interfaces.Authentication;
using PTM.Application.Interfaces.Providers;
using PTM.Application.Interfaces.Repositories;
using PTM.Application.Interfaces.Services;
using PTM.Application.Services;
using PTM.Contracts.Requests;
using PTM.Contracts.Requests.User;
using PTM.Domain.Models;

namespace PTM.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> userRepoMock = new();
    private readonly Mock<ITokenGenerator> tokenGenMock = new();
    private readonly Mock<IRefreshTokenService> refreshTokenServiceMock = new();
    private readonly Mock<IRequestContext> requestContextMock = new();
    private readonly Mock<IRequestContext> requestContextNullMock = new();
    private readonly Mock<IResetPasswordRepository> resetRepoMock = new();
    private readonly Mock<ISmtpEmailSender> smtpMock = new();
    private readonly Mock<IServiceProvider> serviceProviderMock = new();
    private readonly Guid? userIdReq = Guid.NewGuid();
    private readonly AuthService authService;
    private readonly AuthService authServiceFail;

    public AuthServiceTests()
    {
        requestContextMock.Setup(repo => repo.GetUserId()).Returns(userIdReq);
        requestContextNullMock.Setup(repo => repo.GetUserId()).Returns((Guid?)null);

        authService = new AuthService(
            userRepoMock.Object,
            tokenGenMock.Object,
            refreshTokenServiceMock.Object,
            requestContextMock.Object,
            resetRepoMock.Object,
            smtpMock.Object,
            serviceProviderMock.Object
        );
        authServiceFail = new AuthService(
            userRepoMock.Object,
            tokenGenMock.Object,
            refreshTokenServiceMock.Object,
            requestContextNullMock.Object,
            resetRepoMock.Object,
            smtpMock.Object,
            serviceProviderMock.Object
        );
    }

    [Fact]
    public async Task Register_ShouldReturnUserResponseWithTokens()
    {
        var req = new UserRegisterRequest
        {
            Email = "test@test.com",
            Password = "123456@Test",
            PhoneNumber = "09199876542"
        };

        var user = new User { Id = Guid.NewGuid(), Email = req.Email, Password = "hashed", PhoneNumber = req.PhoneNumber };

        userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);

        refreshTokenServiceMock.Setup(r => r.CreateRefreshTokenAsync(It.IsAny<User>()))
        .ReturnsAsync(("raw-refresh", new RefreshToken { Jti = Guid.NewGuid() }));

        tokenGenMock.Setup(t => t.CreateAccessToken(It.IsAny<User>(), It.IsAny<Guid>())).Returns("access-token");

        // Act
        var result = await authService.Register(req);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("raw-refresh");
    }
    [Fact]
    public async Task Login_ShouldReturnUserResponseWithTokens()
    {
        var userId = Guid.NewGuid();
        var rawPassword = "123456@Test";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(rawPassword);
        var req = new UserLoginRequest
        {
            Email = "test@test.com",
            Password = rawPassword,
        };

        var expectedUser = new User
        {
            Id = userId,
            Email = req.Email,
            Password = hashedPassword
        };


        userRepoMock.Setup(r => r.GetUserByEmail(req.Email)).ReturnsAsync(expectedUser);

        refreshTokenServiceMock.Setup(r => r.RevokePreviousToken(It.IsAny<Guid>(), false)).Returns(Task.CompletedTask);

        refreshTokenServiceMock.Setup(r => r.CreateRefreshTokenAsync(It.IsAny<User>()))
        .ReturnsAsync(("raw-refresh", new RefreshToken { Jti = Guid.NewGuid() }));

        tokenGenMock.Setup(t => t.CreateAccessToken(It.IsAny<User>(), It.IsAny<Guid>())).Returns("access-token");

        // Act
        var result = await authService.Login(req);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("test@test.com");
        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("raw-refresh");
    }
    [Fact]
    public async Task Login_ShouldThrowUnauthorizedException_WhenUserDoesNotExist()
    {
        var rawPassword = "123456@Test";
        var req = new UserLoginRequest
        {
            Email = "test@test.com",
            Password = rawPassword,
        };

        userRepoMock.Setup(r => r.GetUserByEmail(req.Email)).ReturnsAsync((User)null!);

        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => authService.Login(req));
        Assert.Contains("Invalid username or password.", exception.Message, StringComparison.OrdinalIgnoreCase);
        refreshTokenServiceMock.Verify(service => service.CreateRefreshTokenAsync(It.IsAny<User>()), Times.Never);
    }
    [Fact]
    public async Task Login_ShouldThrowUnauthorizedException_WhenPasswordIsWrong()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com",
            Password = BCrypt.Net.BCrypt.HashPassword("123")
        };

        var req = new UserLoginRequest
        {
            Email = "test@test.com",
            Password = "abc"
        };


        userRepoMock.Setup(r => r.GetUserByEmail(req.Email)).ReturnsAsync(user);

        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => authService.Login(req));
        Assert.Contains("Invalid username or password.", exception.Message, StringComparison.OrdinalIgnoreCase);
        refreshTokenServiceMock.Verify(service => service.CreateRefreshTokenAsync(It.IsAny<User>()), Times.Never);
    }
    [Fact]
    public async Task RefreshToken_ShouldReturnAccessAndRefreshToken_WhenRawTokenIsValid()
    {

        var req = new RefreshTokenRequest { RefreshToken = "test-token" };


        refreshTokenServiceMock.Setup(rts => rts.GenerateAndRevokeRefreshTokenAsync(req.RefreshToken)).ReturnsAsync(new RevokeResult { AccessToken = "access-token", RefreshToken = "refresh-token" });

        var result = await authService.RefreshToken(req);
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
    }
    [Fact]
    public async Task RefreshToken_ShouldReturnNotFoundException_WhenRawTokenIsInValid()
    {

        var req = new RefreshTokenRequest { RefreshToken = "wrong-token" };

        refreshTokenServiceMock.Setup(rts => rts.GenerateAndRevokeRefreshTokenAsync(req.RefreshToken)).ReturnsAsync((RevokeResult)null!);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => authService.RefreshToken(req));
        Assert.Contains("Your session has expired.", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
    [Fact]
    public async Task UpdatePassword_ShouldBeSuccessfull()
    {
        var userId = userIdReq!.Value;
        var rawPassword = "123456@Test";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(rawPassword);
        var user = new User
        {
            Id = userId,
            Email = "test@test.com",
            Password = hashedPassword
        };
        var req = new UpdatePasswordRequest
        {
            CurrentPassword = rawPassword,
            NewPassword = "abc"
        };
        requestContextMock.Setup(rc => rc.GetUserId()).Returns(userId);
        userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        refreshTokenServiceMock.Setup(rts => rts.RevokePreviousToken(userId, true)).Returns(Task.CompletedTask);

        var result = await authService.UpdatePassword(req);
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
        requestContextNullMock.Setup(rc => rc.GetUserId()).Returns((Guid?)null);
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => authServiceFail.UpdatePassword(req));

        exception.Message.Should().Contain("Unauthorized");
        userRepoMock.Verify(r => r.GetByIdAsync(Guid.NewGuid()), Times.Never);
    }
    [Fact]
    public async Task UpdatePassword_ShouldThrowNotFoundException__WhenUserNotFound()
    {
        var rawPassword = "123456@Test";
        var req = new UpdatePasswordRequest
        {
            CurrentPassword = rawPassword,
            NewPassword = "abc"
        };
        requestContextMock.Setup(rc => rc.GetUserId()).Returns((Guid?)null);
        userRepoMock.Setup(r => r.GetByIdAsync(userIdReq!.Value)).ReturnsAsync((User?)null);
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => authService.UpdatePassword(req));

        exception.Message.Should().Contain("User not found");
        userRepoMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }
    [Fact]
    public async Task UpdatePassword_ShouldThrowNotUnauthorizedAccessException__WhenCurrentPasswordIsWrong()
    {
        var rawPassword = "123456@Test";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(rawPassword);
        var user = new User
        {
            Id = userIdReq!.Value,
            Email = "test@test.com",
            Password = hashedPassword
        };
        var req = new UpdatePasswordRequest
        {
            CurrentPassword = "qwe",
            NewPassword = "abc"
        };
        requestContextMock.Setup(rc => rc.GetUserId()).Returns(userIdReq);
        userRepoMock.Setup(r => r.GetByIdAsync(userIdReq!.Value)).ReturnsAsync(user);
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => authService.UpdatePassword(req));

        exception.Message.Should().Contain("The current password you entered is incorrect.");
        userRepoMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }
    [Fact]
    public async Task Logout_ShouldBeSuccessfull()
    {
        refreshTokenServiceMock.Setup(r => r.RevokePreviousToken(userIdReq!.Value, false)).Returns(Task.CompletedTask);

        // Act
        var result = await authService.Logout();

        // Assert
        result.Should().NotBeNull();
        result.Massage.Should().Be("you logout.");
    }
    [Fact]
    public async Task Logout_ShouldThrowUnauthorizedException__WhenUserIdIsNull()
    {
        requestContextNullMock.Setup(rc => rc.GetUserId()).Returns((Guid?)null);
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => authServiceFail.Logout());
        exception.Message.Should().Contain("Unauthorized");
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

        tokenGenMock.Setup(t => t.HashToken("token")).Returns("access-token");
        resetRepoMock.Setup(r => r.AddAsync(It.IsAny<ResetPassword>())).ReturnsAsync(resetPassword);

        smtpMock.Setup(r => r.SendEmailAsync(req.Email, "Reset Password", "email")).Returns(Task.CompletedTask);

        var result = await authService.ForgotPassword(req);
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
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => authService.ForgotPassword(req));
        exception.Message.Should().Contain("User not found");
        smtpMock.Verify(r => r.SendEmailAsync(req.Email, "Reset Password", "email"), Times.Never);
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
        tokenGenMock.Setup(t => t.HashToken(req.Token)).Returns(hashedToken);
        userRepoMock.Setup(r => r.GetUserByEmail(req.Email)).ReturnsAsync(expectedUser);

        resetRepoMock.Setup(r => r.GetResetPasswordByToken(hashedToken, userId)).ReturnsAsync(resetPassword);
        userRepoMock.Setup(r => r.UpdateAsync(expectedUser)).Returns(Task.CompletedTask);
        resetRepoMock.Setup(r => r.UpdateAsync(resetPassword)).Returns(Task.CompletedTask);
        refreshTokenServiceMock.Setup(rts => rts.RevokePreviousToken(userId, true)).Returns(Task.CompletedTask);

        var result = await authService.ResetPassword(req);
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
        tokenGenMock.Setup(t => t.HashToken(req.Token)).Returns(hashedToken);
        userRepoMock.Setup(r => r.GetUserByEmail(req.Email)).ReturnsAsync((User?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => authService.ResetPassword(req));
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
        tokenGenMock.Setup(t => t.HashToken(req.Token)).Returns(hashedToken);
        userRepoMock.Setup(r => r.GetUserByEmail(req.Email)).ReturnsAsync(expectedUser);

        resetRepoMock.Setup(r => r.GetResetPasswordByToken(hashedToken, userId)).ReturnsAsync((ResetPassword?)null);
       
       var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => authService.ResetPassword(req));

        exception.Message.Should().Contain("The request could not be authorized.");
        userRepoMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }
}
