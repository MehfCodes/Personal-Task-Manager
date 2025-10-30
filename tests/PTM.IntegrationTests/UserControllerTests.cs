using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using PTM.Contracts.Requests;
using PTM.Contracts.Response;

namespace PTM.IntegrationTests;

public class UserControllerTests: IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient client;
    private readonly CustomWebApplicationFactory factory;

    public UserControllerTests(CustomWebApplicationFactory factory)
    {
        client = factory.CreateClient();
        this.factory = factory;
    }
    private async Task AdminLoginAsync()
    {
        var loginReq = new UserLoginRequest
        {
            Email = "user1@test.com",
            Password = "hashed1"
        };

        var loginRes = await client.PostAsJsonAsync("/api/auth/login", loginReq);
        loginRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginBody = await loginRes.Content.ReadFromJsonAsync<ApiResponse<UserResponse>>();
        loginBody.Should().NotBeNull();
        loginBody!.Data!.AccessToken.Should().NotBeNullOrEmpty();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginBody.Data.AccessToken);
    }
    private async Task UserLoginAsync()
    {
        var loginReq = new UserLoginRequest
        {
            Email = "user2@test.com",
            Password = "hashed2"
        };

        var loginRes = await client.PostAsJsonAsync("/api/auth/login", loginReq);
        loginRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginBody = await loginRes.Content.ReadFromJsonAsync<ApiResponse<UserResponse>>();
        loginBody.Should().NotBeNull();
        loginBody!.Data!.AccessToken.Should().NotBeNullOrEmpty();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginBody.Data.AccessToken);
    }
    private async Task<Guid> GetUserId()
    {
        await AdminLoginAsync();
        var res = await client.GetAsync("/api/user");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var resBody = await res.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<UserResponse>>>();
        resBody.Should().NotBeNull();
        return resBody.Data!.Last().Id;
    }
    [Fact]
    public async Task GetUserById_AsAdmin_ShouldReturn200()
    {
        var userId = await GetUserId();

        var response = await client.GetAsync($"/api/User/{userId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserResponse>>();
        result!.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(userId);
    }
    [Fact]
    public async Task GetUserById_AsNormalUser_ShouldReturn401()
    {
        await UserLoginAsync();
        var response = await client.GetAsync($"/api/User/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    [Fact]
    public async Task GetUserById_WithoutToken_ShouldReturn401()
    {
        var response = await client.GetAsync($"/api/User/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    [Fact]
    public async Task UpdateUser_AsAdmin_ShouldReturn200()
    {
        var userId = await GetUserId();
        var request = new UserUpdateRequest
        {
            Email = "updated@test.com",
            PhoneNumber = "09119999999",
            Username = "UpdatedUser"
        };

        var response = await client.PutAsJsonAsync($"/api/User/{userId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserResponse>>();
        result!.Data!.Email.Should().Be("updated@test.com");
    }
    [Fact]
    public async Task UpdateUser_NonExisting_ShouldReturn404()
    {
        var userId = await GetUserId();
        var request = new UserUpdateRequest
        {
            Email = "updated@test.com",
            PhoneNumber = "09119999999",
            Username = "UpdatedUser"
        };

        var response = await client.PutAsJsonAsync($"/api/User/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    [Fact]
    public async Task PromoteUserToAdmin_AsAdmin_ShouldReturn200()
    {
        var userId = await GetUserId();
        var response = await client.PatchAsync($"/api/User/PromoteToAdmin/{userId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<MessageResponse>>();
        result!.Data!.Massage.Should().NotBeNull();
    }

}
