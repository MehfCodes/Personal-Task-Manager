using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using PTM.Application.Exceptions;
using PTM.Application.Interfaces.Repositories;
using PTM.Application.Services;
using PTM.Contracts.Requests;
using PTM.Domain.Models;

namespace PTM.UnitTests.Services;

public class PlanServiceTests
{
    private readonly Mock<IPlanRepository> planRepositoryMock;
    private readonly Mock<IServiceProvider> serviceProviderMock;
    private readonly PlanService planService;
    public PlanServiceTests()
    {
        planRepositoryMock = new Mock<IPlanRepository>();
        serviceProviderMock = new Mock<IServiceProvider>();
        planService = new PlanService(planRepositoryMock.Object, serviceProviderMock.Object);
    }

    [Fact]
    public async Task CreatePlanAsync_ShouldReturnCreatedPlan()
    {
        // Arrange
        var request = new PlanRequest
        {
            Title = "Free",
            Description = "Free Plan",
            Price = 0,
            MaxTasks = 7,
            DurationDays = 7
        };

        Enum.TryParse<PlanTitle>("free", true, out var title);

        var expectedPlan = new Plan
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = request.Description,
            Price = request.Price,
            MaxTasks = request.MaxTasks,
            DurationDays = request.DurationDays
        };

        planRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<Plan>()))
            .ReturnsAsync(expectedPlan);

        var result = await planService.AddAsync(request);
        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Free");
        result.MaxTasks.Should().Be(7);

        planRepositoryMock.Verify(
            repo => repo.AddAsync(It.Is<Plan>(p =>
                p.Title == title &&
                p.Description == request.Description &&
                p.Price == request.Price &&
                p.MaxTasks == request.MaxTasks
        )),
        Times.Once
        );
    }
    [Fact]
    public async Task GetPlanByIdAsync_ShouldReturnPlan_WhenPlanExists()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var expectedPlan = new Plan
        {
            Id = planId,
            Title = PlanTitle.Free,
            Description = "Free plan",
            Price = 0,
            MaxTasks = 7
        };

        planRepositoryMock.Setup(r => r.GetByIdAsync(planId)).ReturnsAsync(expectedPlan);

        // Act
        var result = await planService.GetByIdAsync(planId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPlan.Id, result!.Id);
        Assert.Equal(expectedPlan.Title.ToString(), result.Title);
    }
    [Fact]
    public async Task GetPlanByIdAsync_ShouldThrowNotFoundException_WhenPlanNotExists()
    {
        // Arrange
        var planId = Guid.NewGuid();

        planRepositoryMock.Setup(r => r.GetByIdAsync(planId)).ReturnsAsync((Plan?)null);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => planService.GetByIdAsync(planId));

        // Assert
        Assert.Contains("not found", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
    [Fact]
    public async Task GetAllPlansAsync_ShouldReturnAllPlans()
    {
        // Arrange
        var plans = new List<Plan>
        {
            new Plan{
                Id = Guid.NewGuid(),
                Title = PlanTitle.Free,
                Description = "free plan for testing",
                Price = 0,
                MaxTasks = 5
            },
            new Plan{
                Id = Guid.NewGuid(),
                Title = PlanTitle.Premium,
                Description = "premium plan for testing",
                Price = 10,
                MaxTasks = 50
            },
        };

        planRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(plans);

        // Act
        var result = await planService.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainSingle(p => p.Title == "Free");
        result.Should().ContainSingle(p => p.Title == "Premium");
    }
    [Fact]
    public async Task UpdatePlanAsync_ShouldReturnUpdatedPlan()
    {
        var request = new PlanRequest
        {
            Title = "Free",
            Description = "Free Plan",
            Price = 0,
            MaxTasks = 7,
            DurationDays = 7
        };
        var updateRequest = new PlanUpdateRequest
        {
            Title = "Free",
            Description = "Free Plan - weekly",
            Price = 0,
            MaxTasks = 5,
            DurationDays = 5
        };

        Enum.TryParse<PlanTitle>("free", true, out var title);

        var expectedPlan = new Plan
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = request.Description,
            Price = request.Price,
            MaxTasks = request.MaxTasks,
            DurationDays = request.DurationDays
        };


        planRepositoryMock
        .Setup(repo => repo.GetByIdAsync(expectedPlan.Id))
        .ReturnsAsync(expectedPlan);

        planRepositoryMock
        .Setup(repo => repo.UpdateAsync(It.IsAny<Plan>()))
        .Returns(Task.CompletedTask);


        var updateResult = await planService.UpdateAsync(expectedPlan.Id, updateRequest);

        updateResult.Should().NotBeNull();
        updateResult.DurationDays.Should().Be(5);
        updateResult.MaxTasks.Should().Be(5);

        planRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Plan>()), Times.Once);
    }
    [Fact]
    public async Task DeActiveAsync_ShouldCallRepository_WhenPlanExists()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var existingPlan = new Plan
        {
            Id = planId,
            Title = PlanTitle.Free,
            Description = "Free Plan",
            Price = 0,
            MaxTasks = 5,
            DurationDays = 7
        };

        planRepositoryMock.Setup(r => r.GetByIdAsync(planId)).ReturnsAsync(existingPlan);
        planRepositoryMock.Setup(r => r.UpdateAsync(existingPlan)).Returns(Task.CompletedTask);

        // Act
        await planService.DeActiveAsync(planId);

        // Assert
        planRepositoryMock.Verify(repo => repo.UpdateAsync(existingPlan), Times.Once);
    }
    [Fact]
    public async Task DeActiveAsync_ShouldThrowNotFoundException_WhenPlanDoesNotExist()
    {
        // Arrange
        var planId = Guid.NewGuid();
        planRepositoryMock.Setup(r => r.GetByIdAsync(planId)).ReturnsAsync((Plan)null!);
        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => planService.DeActiveAsync(planId));
        Assert.Contains("not found", exception.Message, StringComparison.OrdinalIgnoreCase);
        planRepositoryMock.Verify(repo => repo.DeActivePlan(It.IsAny<Plan>()), Times.Never);
    }
    [Fact]
    public async Task ActivateAsync_ShouldCallRepository_WhenPlanExists()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var existingPlan = new Plan
        {
            Id = planId,
            Title = PlanTitle.Free,
            Description = "Free Plan",
            Price = 0,
            MaxTasks = 5,
            DurationDays = 7,
            IsActive = false
        };

        planRepositoryMock.Setup(r => r.GetByIdAsync(planId)).ReturnsAsync(existingPlan);
        planRepositoryMock.Setup(r => r.UpdateAsync(existingPlan)).Returns(Task.CompletedTask);

        // Act
        await planService.DeActiveAsync(planId);

        // Assert
        planRepositoryMock.Verify(repo => repo.UpdateAsync(existingPlan), Times.Once);
    }
    [Fact]
    public async Task ActivateAsync_ShouldThrowNotFoundException_WhenPlanDoesNotExist()
    {
        // Arrange
        var planId = Guid.NewGuid();
        planRepositoryMock.Setup(r => r.GetByIdAsync(planId)).ReturnsAsync((Plan)null!);
        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => planService.ActivateAsync(planId));
        Assert.Contains("not found", exception.Message, StringComparison.OrdinalIgnoreCase);
        planRepositoryMock.Verify(repo => repo.ActivatePlan(It.IsAny<Plan>()), Times.Never);
    }
}
