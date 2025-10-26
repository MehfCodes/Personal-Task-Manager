using System;
using FluentAssertions;
using Moq;
using PTM.Application.Exceptions;
using PTM.Application.Interfaces.Policies;
using PTM.Application.Interfaces.Repositories;
using PTM.Application.Interfaces.Services;
using PTM.Application.Services;
using PTM.Contracts.Requests;
using PTM.Contracts.Requests.TaskItem;
using PTM.Contracts.Response;
using PTM.Contracts.Response.UserPlan;
using PTM.Domain.Models;

namespace PTM.UnitTests.Services;

public class TaskItemServiceTests
{
    private readonly Mock<ITaskItemRepository> taskItemRepoMock;
    private readonly Mock<IUserPlanService> userPlanServiceMock;
    private readonly Mock<IRequestContext> requestContextMock;
    private readonly Mock<IServiceProvider> serviceProviderMock;
    private readonly Mock<ICompositePolicy> compositePolicyMock;

    private readonly TaskItemService taskItemService;


    public TaskItemServiceTests()
    {
        taskItemRepoMock = new Mock<ITaskItemRepository>();
        userPlanServiceMock = new Mock<IUserPlanService>();
        requestContextMock = new Mock<IRequestContext>();
        serviceProviderMock = new Mock<IServiceProvider>();
        compositePolicyMock = new Mock<ICompositePolicy>();

        taskItemService = new TaskItemService(
            taskItemRepoMock.Object,
            userPlanServiceMock.Object,
            requestContextMock.Object,
            serviceProviderMock.Object,
            compositePolicyMock.Object
        );
    }

    [Fact]
    public async Task AddAsync_ShouldAddTask_WhenValid()
    {
        var userId = Guid.NewGuid();
        requestContextMock.Setup(repo => repo.GetUserId()).Returns(userId);
        var request = new TaskItemRequest { Title = "Test Task" };
        var userPlan = new UserPlanResponse
        {
            UserId = userId,
            Plan = new PlanResponse { MaxTasks = 5 }
        };
        userPlanServiceMock.Setup(s => s.GetActiveUserPlanByUserId(userId)).ReturnsAsync(It.IsAny<UserPlanResponseDetail>());
        compositePolicyMock.Setup(cp => cp.ValidateAll(userId, It.IsAny<UserPlanResponseDetail>())).Returns(Task.CompletedTask);
        taskItemRepoMock.Setup(r => r.GetTaskCount(userId)).ReturnsAsync(2);

        var addedTask = new TaskItem { Id = Guid.NewGuid(), Title = request.Title };
        taskItemRepoMock.Setup(r => r.AddAsync(It.IsAny<TaskItem>())).ReturnsAsync(addedTask);


        var result = await taskItemService.AddAsync(request);


        result.Should().NotBeNull();
        result.Title.Should().Be(request.Title);
        taskItemRepoMock.Verify(r => r.AddAsync(It.IsAny<TaskItem>()), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ShouldThrowBusinessRuleException_WhenNoActivePlan()
    {
        var userId = Guid.NewGuid();
        requestContextMock.Setup(repo => repo.GetUserId()).Returns(userId);
        var request = new TaskItemRequest { Title = "Test Task" };
        compositePolicyMock
        .Setup(p => p.ValidateAll(userId, It.IsAny<UserPlanResponseDetail>()))
        .ThrowsAsync(new BusinessRuleException("You don't have plan"));
        userPlanServiceMock.Setup(s => s.GetActiveUserPlanByUserId(userId)).ReturnsAsync((UserPlanResponseDetail)null!);

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => taskItemService.AddAsync(request));
        ex.Message.Should().Contain("You don't have plan");
    }
    [Fact]
    public async Task AddAsync_ShouldThrowBusinessRuleException_WhenMaxTasksReached()
    {
        var userId = Guid.NewGuid();
        requestContextMock.Setup(repo => repo.GetUserId()).Returns(userId);
        var request = new TaskItemRequest { Title = "Test Task" };
        var userPlan = new UserPlanResponse
        {
            UserId = userId,
            Plan = new PlanResponse { MaxTasks = 5 }
        };
        compositePolicyMock
        .Setup(p => p.ValidateAll(userId, It.IsAny<UserPlanResponseDetail>()))
        .ThrowsAsync(new BusinessRuleException("You have reached the maximum number of tasks for your plan."));
        userPlanServiceMock.Setup(s => s.GetActiveUserPlanByUserId(userId)).ReturnsAsync(It.IsAny<UserPlanResponseDetail>());
        taskItemRepoMock.Setup(r => r.GetTaskCount(userId)).ReturnsAsync(5);


        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => taskItemService.AddAsync(request));
        exception.Message.Should().Contain("You have reached the maximum number of tasks for your plan.");
    }
    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoTasksExist()
    {

        taskItemRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TaskItem>());
        var result = await taskItemService.GetAllAsync();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
        taskItemRepoMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnMappedTasks_WhenTasksExist()
    {
        var tasks = new List<TaskItem>
        {
            new TaskItem { Id = Guid.NewGuid(), Title = "Task 1" },
            new TaskItem { Id = Guid.NewGuid(), Title = "Task 2" }
        };
        taskItemRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(tasks);

        var result = await taskItemService.GetAllAsync();

        result.Should().NotBeNull();
        result.Count().Should().Be(2);
        result.ElementAt(0).Title.Should().Be("Task 1");
        result.ElementAt(1).Title.Should().Be("Task 2");
        taskItemRepoMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnTask_WhenTaskExists()
    {
        var taskId = Guid.NewGuid();
        var task = new TaskItem { Id = taskId, Title = "Sample Task" };
        taskItemRepoMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(task);

        var result = await taskItemService.GetByIdAsync(taskId);

        result.Should().NotBeNull();
        result.Id.Should().Be(taskId);
        result.Title.Should().Be("Sample Task");
        taskItemRepoMock.Verify(r => r.GetByIdAsync(taskId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowNotFoundException_WhenTaskDoesNotExist()
    {

        var taskId = Guid.NewGuid();
        taskItemRepoMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync((TaskItem?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => taskItemService.GetByIdAsync(taskId));

        exception.Message.Should().Contain("Task");
        taskItemRepoMock.Verify(r => r.GetByIdAsync(taskId), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedTask_WhenTaskExists()
    {
        var taskId = Guid.NewGuid();
        var existingTask = new TaskItem { Id = taskId, Title = "Old Title" };
        var updateRequest = new TaskItemUpdateRequest { Title = "New Title" };

        taskItemRepoMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(existingTask);
        taskItemRepoMock.Setup(r => r.UpdateAsync(It.IsAny<TaskItem>())).Returns(Task.CompletedTask);

        var result = await taskItemService.UpdateAsync(taskId, updateRequest);

        result.Should().NotBeNull();
        result.Id.Should().Be(taskId);
        result.Title.Should().Be("New Title");
        taskItemRepoMock.Verify(r => r.GetByIdAsync(taskId), Times.Once);
        taskItemRepoMock.Verify(r => r.UpdateAsync(It.Is<TaskItem>(t => t.Id == taskId && t.Title == "New Title")), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowNotFoundException_WhenTaskDoesNotExist()
    {
        var taskId = Guid.NewGuid();
        var updateRequest = new TaskItemUpdateRequest { Title = "New Title" };

        taskItemRepoMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync((TaskItem?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => taskItemService.UpdateAsync(taskId, updateRequest));

        exception.Message.Should().Contain("Task");
        taskItemRepoMock.Verify(r => r.GetByIdAsync(taskId), Times.Once);
        taskItemRepoMock.Verify(r => r.UpdateAsync(It.IsAny<TaskItem>()), Times.Never);
    }
    [Fact]
    public async Task DeleteAsync_ShouldDeleteTask_WhenTaskExists()
    {
        var taskId = Guid.NewGuid();
        var task = new TaskItem { Id = taskId, Title = "title" };
        taskItemRepoMock.Setup(r => r.DeleteAsync(taskId)).ReturnsAsync(task);

        await taskItemService.DeleteAsync(taskId);

        taskItemRepoMock.Verify(r => r.DeleteAsync(taskId), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowNotFoundException_WhenTaskDoesNotExist()
    {
        var taskId = Guid.NewGuid();
        taskItemRepoMock.Setup(r => r.DeleteAsync(taskId)).ReturnsAsync((TaskItem?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => taskItemService.DeleteAsync(taskId));
    }

    [Fact]
    public async Task ChangeStatus_ShouldUpdateStatus_WhenTaskExists()
    {
        var taskId = Guid.NewGuid();
        var task = new TaskItem { Id = taskId, Title = "title", Status = Status.Done };
        var request = new ChangeStatusRequest { Status = "Todo" };

        taskItemRepoMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(task);
        taskItemRepoMock.Setup(r => r.UpdateAsync(It.IsAny<TaskItem>())).Returns(Task.CompletedTask);

        var result = await taskItemService.ChangeStatus(taskId, request);

        result.Should().NotBeNull();
        result.Status.Should().Be("Todo");
        taskItemRepoMock.Verify(r => r.UpdateAsync(It.Is<TaskItem>(t => t.Status == Status.Todo)), Times.Once);
    }

    [Fact]
    public async Task ChangeStatus_ShouldThrowNotFoundException_WhenTaskDoesNotExist()
    {
        var taskId = Guid.NewGuid();
        var request = new ChangeStatusRequest { Status = "Todo" };
        taskItemRepoMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync((TaskItem?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => taskItemService.ChangeStatus(taskId, request));
    }

    [Fact]
    public async Task ChangeStatus_ShouldHandleInvalidStatusString_Gracefully()
    {
        var taskId = Guid.NewGuid();
        var task = new TaskItem { Id = taskId, Title = "tiitle", Status = Status.Todo };
        var request = new ChangeStatusRequest { Status = "InvalidStatus" };

        taskItemRepoMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(task);
        taskItemRepoMock.Setup(r => r.UpdateAsync(It.IsAny<TaskItem>())).Returns(Task.CompletedTask);

        var result = await taskItemService.ChangeStatus(taskId, request);

        result.Should().NotBeNull();
        result.Status.Should().Be("Todo");
        taskItemRepoMock.Verify(r => r.UpdateAsync(It.Is<TaskItem>(t => t.Status == Status.Todo)), Times.Once);
    }
    [Fact]
    public async Task ChangePriority_ShouldUpdatePriority_WhenValid()
    {
        var taskId = Guid.NewGuid();
        var existingTask = new TaskItem { Id = taskId, Title="title", Priority = Priority.Low };
        taskItemRepoMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(existingTask);
        taskItemRepoMock.Setup(r => r.UpdateAsync(It.IsAny<TaskItem>())).Returns(Task.CompletedTask);

        var request = new ChangePriorityRequest { Priority = "High" };

        var result = await taskItemService.ChangePriority(taskId, request);

        result.Should().NotBeNull();
        result.Priority.Should().Be(Priority.High.ToString());
        taskItemRepoMock.Verify(r => r.UpdateAsync(It.Is<TaskItem>(t => t.Priority == Priority.High)), Times.Once);
    }

    [Fact]
    public async Task ChangePriority_ShouldThrowNotFoundException_WhenTaskDoesNotExist()
    {
       var taskId = Guid.NewGuid();
        taskItemRepoMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync((TaskItem)null!);

        var request = new ChangePriorityRequest { Priority = "High" };

        await Assert.ThrowsAsync<NotFoundException>(() => taskItemService.ChangePriority(taskId, request));
    }

    [Fact]
    public async Task ChangePriority_ShouldSetDefaultPriority_WhenInvalidPriorityString()
    {
        var taskId = Guid.NewGuid();
        var existingTask = new TaskItem { Id = taskId, Title="title", Priority = Priority.Low };
        taskItemRepoMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(existingTask);
        taskItemRepoMock.Setup(r => r.UpdateAsync(It.IsAny<TaskItem>())).Returns(Task.CompletedTask);

        var request = new ChangePriorityRequest { Priority = "InvalidPriority" };

        var result = await taskItemService.ChangePriority(taskId, request);

        result.Should().NotBeNull();
        result.Priority.Should().Be(Priority.Low.ToString());
        taskItemRepoMock.Verify(r => r.UpdateAsync(It.Is<TaskItem>(t => t.Priority == Priority.Low)), Times.Once);
    }
}
