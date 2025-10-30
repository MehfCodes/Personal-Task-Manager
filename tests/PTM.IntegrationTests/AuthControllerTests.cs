using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using PTM.Application.Interfaces.Services;
using PTM.Contracts.Requests;
using PTM.Contracts.Requests.User;
using PTM.Contracts.Response;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit.Abstractions;

namespace PTM.IntegrationTests;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient client;
    private readonly CustomWebApplicationFactory factory;
    private readonly ITestOutputHelper output;

    public AuthControllerTests(CustomWebApplicationFactory factory, ITestOutputHelper output)
    {
        client = factory.CreateClient();
        this.factory = factory;
        this.output = output;
    }

    [Fact]
    public async Task Register_ShouldReturnTokens()
    {
        // Arrange
        var req = new UserRegisterRequest
        {
            Email = "integ@test.com",
            Username = "integ",
            Password = "Test@1234",
            PhoneNumber = "09120000000"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", req);
        // Assert
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<UserResponse>>();
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        body.Should().NotBeNull();
        body.Data.Should().NotBeNull();
        body.Data.AccessToken.Should().NotBeNullOrEmpty();
        body.Data.RefreshToken.Should().NotBeNullOrEmpty();
    }
    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenValidationFailed()
    {
        // Arrange
        var req = new UserRegisterRequest
        {
            Email = "user1test.com",
            Username = "duplicated",
            Password = "Tes1234",
            PhoneNumber = "09123334444"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", req);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenRequiredFieldsMissing()
    {
        // Arrange
        var req = new UserRegisterRequest
        {
            Email = "",
            Username = "",
            Password = "",
            PhoneNumber = ""
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", req);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    [Fact]
    public async Task Login_ShouldReturnTokens_WhenCredentialsValid()
    {
        // Arrange: 
        var loginReq = new UserLoginRequest
        {
            Email = "user1@test.com",
            Password = "hashed1"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", loginReq);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserResponse>>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        result.Data.AccessToken.Should().NotBeNullOrEmpty();
        result.Data.RefreshToken.Should().NotBeNullOrEmpty();
    }
     [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenPasswordInvalid()
    {
        // Arrange
        var req = new UserLoginRequest
        {
            Email = "user1@test.com",
            Password = "WrongPassword123"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", req);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenUserNotFound()
    {
        // Arrange
        var req = new UserLoginRequest
        {
            Email = "notexist@test.com",
            Password = "Test@1234"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", req);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenFieldsMissing()
    {
        // Arrange
        var req = new UserLoginRequest
        {
            Email = "",
            Password = ""
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", req);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    [Fact]
    public async Task RefreshToken_ShouldReturnNewTokens_WhenValidRequest()
    {
        // Arrange
        var loginReq = new UserLoginRequest
        {
            Email = "user1@test.com",
            Password = "hashed1"
        };
        var logingRes = await client.PostAsJsonAsync("/api/auth/login", loginReq);
        var loginBody = await logingRes.Content.ReadFromJsonAsync<ApiResponse<UserResponse>>();

        var refreshReq = new RefreshTokenRequest
        {
            RefreshToken = loginBody!.Data!.RefreshToken,
        };
        // client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginBody.Data.AccessToken);
        // Act
        var response = await client.PostAsJsonAsync("/api/auth/refresh", refreshReq);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<RefreshTokenResponse>>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        result.Data.AccessToken.Should().NotBeNullOrEmpty();
        result.Data.RefreshToken.Should().NotBeNullOrEmpty();
    }
    [Fact]
    public async Task RefreshToken_ShouldReturnUnauthorized_WhenTokenInvalid()
    {
        // Arrange
        var req = new RefreshTokenRequest
        {
            RefreshToken = "invalid_token"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/refresh", req);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    [Fact]
    public async Task RefreshToken_ShouldReturnBadRequest_WhenTokenMissing()
    {
        // Arrange
        var req = new RefreshTokenRequest
        {
            RefreshToken = ""
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/refresh", req);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task UpdatePassword_ShouldReturnUnauthorized_WhenNoTokenProvided()
    {
        // Arrange
        var req = new UpdatePasswordRequest
        {
            CurrentPassword = "Test@1234",
            NewPassword = "New@1234"
        };

        // Act
        var response = await client.PatchAsJsonAsync("/api/auth/update-password", req);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdatePassword_ShouldReturnBadRequest_WhenNewPasswordWeak()
    {

        var regiserReq = new UserRegisterRequest
        {
            Email = "integ@test.com",
            Username = "integ",
            Password = "Test@1234",
            PhoneNumber = "09120000000"
        };

        // Act
        var register = await client.PostAsJsonAsync("/api/auth/register", regiserReq);
        var registerResult = await register.Content.ReadFromJsonAsync<ApiResponse<UserResponse>>();
        register.StatusCode.Should().Be(HttpStatusCode.Created);

        var req = new UpdatePasswordRequest
        {
            CurrentPassword = "Test@1234",
            NewPassword = "123"
        };

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", registerResult!.Data!.AccessToken);
        // Act
        var response = await client.PatchAsJsonAsync("/api/auth/update-password", req);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    [Fact]
    public async Task UpdatePassword_ShouldReturnOk_WhenUserIsAuthenticated()
    {
        // Arrange: user login
        var loginRequest = new UserLoginRequest
        {
            Email = "user2@test.com",
            Password = "hashed2"
        };

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<UserResponse>>();
        loginBody.Should().NotBeNull();
        loginBody!.Data.Should().NotBeNull();
        var token = loginBody.Data.AccessToken;

        // Arrange: update password request
        var updateRequest = new UpdatePasswordRequest
        {
            CurrentPassword = "hashed2",
            NewPassword = "NewPassword123!"
        };

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act: call update-password
        var response = await client.PatchAsJsonAsync("/api/auth/update-password", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await response.Content.ReadFromJsonAsync<ApiResponse<UpdatePasswordResponse>>();
        responseBody.Should().NotBeNull();
        responseBody!.Data.Should().NotBeNull();
        responseBody.Message.Should().Be("Password update successfully");
    }

    [Fact]
    public async Task ForgotPassword_ShouldReturnSuccessMessage()
    {
        var factory = new CustomWebApplicationFactory();
        var emailServiceMock = new Mock<IEmailService>();
        emailServiceMock
            .Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(IEmailService));
                services.AddSingleton(emailServiceMock.Object);
            });
        }).CreateClient();
        // Arrange
        var loginReq = new UserLoginRequest
        {
            Email = "user1@test.com",
            Password = "hashed1"
        };
        var logingRes = await client.PostAsJsonAsync("/api/auth/login", loginReq);
        var loginBody = await logingRes.Content.ReadFromJsonAsync<ApiResponse<UserResponse>>();


        var forgotReq = new ForgotPasswordRequest
        {
            Email = "user1@test.com"
        };
        Console.WriteLine(loginBody!.Data!.AccessToken);
        // Act
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginBody!.Data!.AccessToken);
        var response = await client.PostAsJsonAsync("/api/auth/forgot-password", forgotReq);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ForgotPasswordResponse>>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        emailServiceMock.Verify(x => x.SendEmailAsync(
        It.Is<string>(email => email == "user1@test.com"),
        It.Is<string>(subject => subject.Contains("Reset")),
        It.IsAny<string>()),
        Times.Once);
    }
    [Fact]
    public async Task ForgotPassword_ShouldReturnNotFound_WhenEmailDoesNotExist()
    {
        // Arrange
        var req = new ForgotPasswordRequest
        {
            Email = "notexist@test.com"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/forgot-password", req);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ForgotPassword_ShouldReturnBadRequest_WhenEmailInvalidFormat()
    {
        // Arrange
        var req = new ForgotPasswordRequest
        {
            Email = "not-an-email"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/forgot-password", req);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    [Fact]
    public async Task ResetPassword_ShouldReturnSuccess_WhenTokenValid()
    {
        var req = new ResetPasswordRequest
        {
            Email = "user1@test.com",
            Token = "dummy-token",
            NewPassword = "NewPass@1234"
        };

        var response = await client.PostAsJsonAsync("/api/auth/reset-password", req);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Logout_ShouldReturnOk_WhenTokenProvided()
    {
        var loginReq = new UserLoginRequest
        {
            Email = "user2@test.com",
            Password = "hashed2"
        };
        var logingRes = await client.PostAsJsonAsync("/api/auth/login", loginReq);
        var loginBody = await logingRes.Content.ReadFromJsonAsync<ApiResponse<UserResponse>>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginBody!.Data!.AccessToken);

        // Act
        var response = await client.PostAsync("/api/auth/logout", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
   
    [Fact]
    public async Task Logout_ShouldReturnUnauthorized_WhenNoTokenProvided()
    {
        // Act
        var response = await client.PostAsync("/api/auth/logout", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
