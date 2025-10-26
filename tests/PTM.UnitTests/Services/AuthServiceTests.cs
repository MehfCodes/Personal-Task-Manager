
using FluentAssertions;
using Microsoft.Extensions.Logging;
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
using PTM.Contracts.Response.Token;
using PTM.Domain.Models;

namespace PTM.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> userRepoMock = new();
    private readonly Mock<ITokenService> tokenService = new();
    private readonly Mock<IRefreshTokenService> refreshTokenServiceMock = new();
    private readonly Mock<IPasswordHasher> passwordHasherMock = new();
    private readonly Mock<IRequestContext> requestContextMock = new();
    private readonly Mock<IServiceProvider> serviceProviderMock = new();
    private readonly Mock<ILogger<AuthService>> loggerMock = new();
    private readonly AuthService authService;

    public AuthServiceTests()
    {
        authService = new AuthService(
            userRepoMock.Object,
            tokenService.Object,
            refreshTokenServiceMock.Object,
            requestContextMock.Object,
            serviceProviderMock.Object,
            passwordHasherMock.Object,
            loggerMock.Object
            );
    }

    [Fact]
    public async Task Register_ShouldReturnUserResponseWithTokens()
    {
        var rawPassword = "pass";
        var req = new UserRegisterRequest
        {
            Email = "test@test.com",
            Username="username",
            PhoneNumber = "09199876542",
            Password = rawPassword,
        };

        var user = new User {
            Id = Guid.NewGuid(),
            Email = req.Email,
            Password = "hashed",
            PhoneNumber = req.PhoneNumber
        };
        passwordHasherMock.Setup(ph => ph.HashPassword(req.Password)).Returns("hashed-password");
        // passwordHasherMock.Setup(ph => ph.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);
        tokenService.Setup(ts => ts.GenerateTokenPair(It.IsAny<User>()))
        .ReturnsAsync(new TokenPair
        {
            AccessToken = "access-token",
            RefreshToken = "raw-refresh"
        });
        // Act
        var result = await authService.Register(req);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(req.Email);
        result.RefreshToken.Should().Be("raw-refresh");
        result.AccessToken.Should().Be("access-token");
    }
    [Fact]
    public async Task Login_ShouldReturnUserResponseWithTokens()
    {
        var userId = Guid.NewGuid();
        var rawPassword = "123456@Test";
        passwordHasherMock.Setup(ph => ph.HashPassword(rawPassword)).Returns((string pw) => "hashed_" + pw);
        var req = new UserLoginRequest
        {
            Email = "test@test.com",
            Password = rawPassword,
        };

        var expectedUser = new User
        {
            Id = userId,
            Email = req.Email,
            Password = "hashedPassword"
        };


        userRepoMock.Setup(r => r.GetUserByEmail(req.Email)).ReturnsAsync(expectedUser);
        passwordHasherMock.Setup(ph => ph.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        refreshTokenServiceMock.Setup(r => r.RevokePreviousToken(It.IsAny<Guid>(), false)).Returns(Task.CompletedTask);
        tokenService.Setup(ts => ts.GenerateTokenPair(It.IsAny<User>()))
        .ReturnsAsync(new TokenPair
        {
            AccessToken = "access-token",
            RefreshToken = "raw-refresh"
        });
        // Act
        var result = await authService.Login(req);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("test@test.com");
        result.RefreshToken.Should().Be("raw-refresh");
        result.AccessToken.Should().Be("access-token");
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
        tokenService.Verify(service => service.CreateRefreshTokenAsync(It.IsAny<User>()), Times.Never);
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
        tokenService.Verify(service => service.CreateRefreshTokenAsync(It.IsAny<User>()), Times.Never);
    }
    [Fact]
    public async Task RefreshToken_ShouldReturnAccessAndRefreshToken_WhenRawTokenIsValid()
    {

        var req = new RefreshTokenRequest
        {
            RefreshToken = "test-token" 
        };

        var userId = Guid.NewGuid();
        requestContextMock.Setup(repo => repo.GetUserId()).Returns(userId);
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
    public async Task Logout_ShouldBeSuccessfull()
    {
                var userId = Guid.NewGuid();
        requestContextMock.Setup(repo => repo.GetUserId()).Returns(userId);
        refreshTokenServiceMock.Setup(r => r.RevokePreviousToken(userId, false)).Returns(Task.CompletedTask);

        // Act
        var result = await authService.Logout();

        // Assert
        result.Should().NotBeNull();
        result.Massage.Should().Be("you logout.");
    }
    [Fact]
    public async Task Logout_ShouldThrowUnauthorizedException__WhenUserIdIsNull()
    {
        requestContextMock.Setup(rc => rc.GetUserId()).Returns((Guid?)null);
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => authService.Logout());
        exception.Message.Should().Contain("Unauthorized");
    }
}
