using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using PTM.Contracts.Requests;
using PTM.Contracts.Requests.TaskItem;
using PTM.Contracts.Requests.UserPlan;
using PTM.Contracts.Response;
using PTM.Contracts.Response.TaskItem;
using PTM.Contracts.Response.UserPlan;
using PTM.Domain.Models;

namespace PTM.IntegrationTests;

public class TaskItemControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient client;
    private readonly CustomWebApplicationFactory factory;

    public TaskItemControllerTests(CustomWebApplicationFactory factory)
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
    private async Task<TaskItemResponse> SelectTaskItem()
    {
        await LoginAsync();
        var tasksRes = await client.GetAsync("/api/taskitem");
        tasksRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var allTasks = await tasksRes.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<TaskItemResponse>>>();
        allTasks.Should().NotBeNull();
        allTasks.Data.Should().NotBeNull();
        var taskItem = allTasks.Data.First();
        return taskItem;
    }
    [Fact]
    public async Task Add_ShouldReturn201_WhenRequestIsValid()
    {
        // Arrange
        await RegisterUserAsync();
        var plansResponse = await client.GetAsync("/api/plan");
        var plansBody = await plansResponse.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<PlanResponse>>>();
        var firstPlanId = plansBody!.Data!.Where(p => p.Title.ToString() == "Premium").First().Id;
        var purchaseReq = new UserPlanRequest { PlanId = firstPlanId };
        var purchaseResponse = await client.PostAsJsonAsync("/api/userplan", purchaseReq);
        purchaseResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        // var purchasePlanBody = await purchaseResponse.Content.ReadFromJsonAsync<ApiResponse<UserPlanResponseDetail>>();

        var request = new TaskItemRequest
        {
            Title = "Integration Test Task",
            Description = "Testing add endpoint",
            Priority = "High",
            Status = "Todo"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/taskitem", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<TaskItemResponse>>();
        body.Should().NotBeNull();
        body!.Data!.Title.Should().Be(request.Title);
        body.Message.Should().Be("Task created successfully");
    }
    [Fact]
    public async Task Add_ShouldReturn400_WhenTitleIsMissing()
    {
        await LoginAsync();
        // Arrange
        var invalidRequest = new TaskItemRequest
        {
            Title = "",
            Description = "Missing title test",
            Priority = "Low"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/taskitem", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    [Fact]
    public async Task GetAll_ShouldReturn200_WhenTasksExists()
    {
        await LoginAsync();
        // Arrange
        var tasksRes = await client.GetAsync("/api/taskitem");
        tasksRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var allTasks = await tasksRes.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<TaskItemResponse>>>();
        allTasks.Should().NotBeNull();
        allTasks.Data.Should().NotBeNull();
    }
    [Fact]
    public async Task Get_ShouldReturn200_WhenTaskExists()
    {
        // Arrange
        var task = await SelectTaskItem();
        var taskId = task.Id;
        var taskTitle = task.Title;

        // Act
        var response = await client.GetAsync($"/api/taskitem/{taskId}");
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<TaskItemResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.Data!.Title.Should().Be(taskTitle);
    }
    [Fact]
    public async Task Get_ShouldReturn404_WhenTaskDoesNotExist()
    {
        await LoginAsync();
        var response = await client.GetAsync($"/api/taskitem/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
     [Fact]
    public async Task Update_ShouldReturn200_WhenTaskExists()
    {
        var task = await SelectTaskItem();
        var taskId = task.Id;

        var updateReq = new TaskItemUpdateRequest
        {
            Title = "Updated title",
            Description = "Updated description",
            Priority = "High"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/taskitem/{taskId}", updateReq);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<TaskItemResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.Data!.Title.Should().Be(updateReq.Title);
    }
    [Fact]
    public async Task Update_ShouldReturn404_WhenTaskDoesNotExist()
    {
        await LoginAsync();
        var updateReq = new TaskItemUpdateRequest
        {
            Title = "Invalid",
            Description = "Invalid",
            Priority = "Low"
        };

        var response = await client.PutAsJsonAsync($"/api/taskitem/{Guid.NewGuid()}", updateReq);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    [Fact]
    public async Task Delete_ShouldReturn200_WhenTaskExists()
    {
        // Arrange
        var task = await SelectTaskItem();
        var taskId = task.Id;
        // Act
        var response = await client.DeleteAsync($"/api/taskitem/{taskId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    [Fact]
    public async Task Delete_ShouldReturn404_WhenTaskDoesNotExist()
    {
        await LoginAsync();
        var response = await client.DeleteAsync($"/api/taskitem/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    [Fact]
    public async Task ChangeStatus_ShouldReturn200_WhenTaskExists()
    {
        // Arrange
        var task = await SelectTaskItem();
        var taskId = task.Id;
        var patchReq = new ChangeStatusRequest { Status = Status.Done.ToString() };

        // Act
        var response = await client.PatchAsJsonAsync($"/api/taskitem/{taskId}/status", patchReq);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ChangeStatusResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.Data!.Status.Should().Be(Status.Done.ToString());
    }
    [Fact]
    public async Task ChangePriority_ShouldReturn200_WhenTaskExists()
    {
        // Arrange
        var task = await SelectTaskItem();
        var taskId = task.Id;
        var patchReq = new ChangePriorityRequest { Priority = "High" };

        // Act
        var response = await client.PatchAsJsonAsync($"/api/taskitem/{taskId}/priority", patchReq);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ChangePriorityResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.Data!.Priority.Should().Be("High");
    }
}
