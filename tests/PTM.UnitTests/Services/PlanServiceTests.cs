using System;
using FluentAssertions;
using Moq;
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
}
