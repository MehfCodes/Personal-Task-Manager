using System;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PTM.Contracts.Response;

namespace PTM.IntegrationTests;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient client;
    private readonly CustomWebApplicationFactory factory;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        client = factory.CreateClient();
        this.factory = factory;
    }

    [Fact]
    public async Task Register_ShouldReturnTokens()
    {
        // Arrange
        var req = new
        {
            Email = "integ@test.com",
            Username = "integ",
            Password = "Test@1234",
            PhoneNumber = "09120000000"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", req);

        // Assert
        var body = await response.Content.ReadFromJsonAsync<UserResponse>();
          Console.WriteLine(body); 
        // response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().NotBeNull();
        body.AccessToken.Should().NotBeNullOrEmpty();
        body.RefreshToken.Should().NotBeNullOrEmpty();
    }
}
