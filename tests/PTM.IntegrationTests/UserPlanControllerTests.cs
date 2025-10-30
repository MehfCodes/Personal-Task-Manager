using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using PTM.Contracts.Requests;
using PTM.Contracts.Requests.UserPlan;
using PTM.Contracts.Response;
using PTM.Contracts.Response.UserPlan;

namespace PTM.IntegrationTests;

public class UserPlanControllerTests: IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory factory;
        private readonly HttpClient client;

    public UserPlanControllerTests(CustomWebApplicationFactory factory)
    {
        this.factory = factory;
        client = factory.CreateClient();
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
    private async Task<Guid> Purchase()
    {
        await UserLoginAsync();
        var plansRes = await client.GetAsync("/api/plan");
        var plansBody = await plansRes.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<PlanResponse>>>();
        var planId = plansBody!.Data!.First().Id;
        var request = new UserPlanRequest
        {
            PlanId = planId
        };

        var purchasedPlan = await client.PostAsJsonAsync("/api/UserPlan", request);
        var purchasedPlanRes = await purchasedPlan.Content.ReadFromJsonAsync<ApiResponse<UserPlanResponseDetail>>();
        return purchasedPlanRes!.Data!.Id;
    }
    [Fact]
    public async Task AddUserPlan_AsUser_ShouldReturn201()
    {
        await UserLoginAsync();
        var plansRes = await client.GetAsync("/api/plan");
        var plansBody = await plansRes.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<PlanResponse>>>();
        var planId = plansBody!.Data!.First().Id;
        var request = new UserPlanRequest
        {
            PlanId = planId
        };

        var response = await client.PostAsJsonAsync("/api/UserPlan", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserPlanResponseDetail>>();
        result!.Data.Should().NotBeNull();
        result.Data.PlanId.Should().Be(request.PlanId);
    }
    [Fact]
    public async Task AddUserPlan_InvalidPlan_ShouldReturn404()
    {
        await UserLoginAsync();
        var request = new UserPlanRequest
        {
            PlanId = Guid.NewGuid()
        };

        var response = await client.PostAsJsonAsync("/api/UserPlan", request);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    [Fact]
    public async Task GetUserPlan_AsUser_ShouldReturn200()
    {
        var id = await Purchase();
        var response = await client.GetAsync($"/api/UserPlan/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserPlanResponseDetail>>();
        result!.Data!.Id.Should().Be(id);
    }
    [Fact]
    public async Task DeactiveUserPlan_AsAdmin_ShouldReturn200()
    {
        var purchasedPlanId = await Purchase();

        var response = await client.PatchAsync($"/api/UserPlan/{purchasedPlanId}/deactive", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<MessageResponse>>();
        result!.Data!.Massage.Should().Contain("deactivated");
    }
    [Fact]
    public async Task DeactiveUserPlan_NotFound_ShouldReturn404()
    {
        await UserLoginAsync();
        var response = await client.PatchAsync($"/api/UserPlan/{Guid.NewGuid()}/deactive", null);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    [Fact]
    public async Task AnyEndpoint_WithoutToken_ShouldReturn401()
    {
        var response = await client.PostAsJsonAsync("/api/UserPlan", new UserPlanRequest { PlanId = Guid.NewGuid() });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }


}
