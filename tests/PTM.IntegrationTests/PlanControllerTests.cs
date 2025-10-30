using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using PTM.Contracts.Requests;
using PTM.Contracts.Response;

namespace PTM.IntegrationTests;

public class PlanControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient client;
    private readonly CustomWebApplicationFactory factory;

    public PlanControllerTests(CustomWebApplicationFactory factory)
    {
        client = factory.CreateClient();
        this.factory = factory;
    }
    private async Task LoginAsync()
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
    private async Task RegisterUserAsync()
    {
        var req = new UserRegisterRequest
        {
            Email = "integ@test.com",
            Username = "integ",
            Password = "Test@1234",
            PhoneNumber = "09120000000"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", req);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<UserResponse>>();
        body.Should().NotBeNull();
        body!.Data!.AccessToken.Should().NotBeNullOrEmpty();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body.Data.AccessToken);
    }
    [Fact]
    public async Task AddPlan_ShouldReturnCreated_WhenAdmin()
    {
        // Arrange
        await LoginAsync();

        var request = new PlanRequest
        {
            Title = "Business",
            Description = "Access to all features",
            Price = 40,
            DurationDays = 365
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/plan", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PlanResponse>>();
        result.Should().NotBeNull();
        result!.Data!.Title.Should().Be("Business");
    }
    [Fact]
    public async Task Add_ShouldReturnBadRequest_WhenInvalidData()
    {
        // Arrange
        await LoginAsync();
        var invalidRequest = new PlanRequest
        {
            Title = "Business",
            Description = "Access to all features",
            Price = 40,
            DurationDays = 30
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/plan", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    [Fact]
    public async Task AddPlan_ShouldReturnForbidden_WhenUserIsNotAdmin()
    {
        // Arrange
        await RegisterUserAsync();
        var request = new PlanRequest
        {
            Title = "Business",
            Description = "Access to all features",
            Price = 40,
            DurationDays = 365
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/plan", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    [Fact]
    public async Task GetPlan_ShouldReturnOk_WhenPlanExists()
    {
        // Arrange
        await LoginAsync();
        var plansRes = await client.GetAsync("/api/plan");
        var plansBody = await plansRes.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<PlanResponse>>>();
        var planId = plansBody!.Data!.First().Id;

        // Act
        var response = await client.GetAsync($"/api/plan/{planId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PlanResponse>>();
        result!.Data!.Id.Should().Be(planId);
    }
    [Fact]
    public async Task GetPlan_ShouldReturnNotFound_WhenPlanDoesNotExist()
    {
        await LoginAsync();
        // Act
        var response = await client.GetAsync($"/api/plan/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    [Fact]
    public async Task GetAllPlans_ShouldReturnOk_WhenPlansExists()
    {
        // Arrange
        await LoginAsync();
        // Act
        var plansRes = await client.GetAsync("/api/plan");
        // Assert
        plansRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var plansBody = await plansRes.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<PlanResponse>>>();
        plansBody!.Data!.Should().NotBeNull();
    }
    [Fact]
    public async Task Update_ShouldReturnOk_WhenAdminAndValidRequest()
    {
        // Arrange
        await LoginAsync();
        var plansRes = await client.GetAsync("/api/plan");
        var plansBody = await plansRes.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<PlanResponse>>>();
        var planId = plansBody!.Data!.First().Id;
        var request = new PlanUpdateRequest
        {
            Id = planId,
            Description = "Access to all business features",
            Title= "Business",
            Price= 90,
            DurationDays= 365,
        };
        // Act
        var response = await client.PutAsJsonAsync($"/api/plan/{planId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PlanResponse>>();
        result.Should().NotBeNull();
        result!.Data!.Description.Should().Be("Access to all business features");
    }
    [Fact]
    public async Task DeActivePlan_ShouldReturnOk_WhenAdmin()
    {
        // Arrange
        await LoginAsync();

        var plansRes = await client.GetAsync("/api/plan");
        var plansBody = await plansRes.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<PlanResponse>>>();
        var planId = plansBody!.Data!.First().Id;

        // Act
        var response = await client.PatchAsync($"/api/plan/{planId}/deactive", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<MessageResponse>>();
        result!.Data!.Massage.Should().Contain("Deactived");
    }
    [Fact]
    public async Task DeActivePlan_ShouldReturnNotFound_WhenInvalidId()
    {
        await LoginAsync();
        var response = await client.PatchAsync($"/api/plan/{Guid.NewGuid()}/deactive", null);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    [Fact]
    public async Task ActivePlan_ShouldReturnOk_WhenAdmin()
    {
        // Arrange
        await LoginAsync();

        var plansRes = await client.GetAsync("/api/plan");
        var plansBody = await plansRes.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<PlanResponse>>>();
        var planId = plansBody!.Data!.First().Id;

        // Act
        var response = await client.PatchAsync($"/api/plan/{planId}/active", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<MessageResponse>>();
        result!.Data!.Massage.Should().Contain("Actived");
    }
}
