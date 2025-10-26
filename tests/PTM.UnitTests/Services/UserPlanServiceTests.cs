using System;
using System.Linq.Expressions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PTM.Application.Exceptions;
using PTM.Application.Interfaces;
using PTM.Application.Interfaces.Policies;
using PTM.Application.Interfaces.Repositories;
using PTM.Application.Interfaces.Services;
using PTM.Application.Policies;
using PTM.Application.Services;
using PTM.Contracts.Response.UserPlan;
using PTM.Domain.Models;
using PTM.Infrastructure.Repository;

namespace PTM.UnitTests.Services;

public class UserPlanServiceTests
{
    private readonly Mock<IPlanRepository> planRepoMock = new();
    private readonly Mock<IUserRepository> userRepoMock = new();
    private readonly Mock<IServiceProvider> serviceProviderMock = new();
    private readonly Mock<IBaseRepository<UserPlan>> userPlanRepoMock = new();
    private readonly Mock<IRequestContext> requestContextMock = new();
    private readonly UserPlanService userPlanService;
    private readonly Guid userId = Guid.NewGuid();
    private readonly Mock<ILogger<UserPlanService>> loggerMock = new();
    private readonly Mock<IUserPlanPolicy<Guid>> userPlanPolicy = new();
    private readonly Mock<IUserPlanPolicy<UserPlan>> expirationPolicy = new();
    public UserPlanServiceTests()
    {
        userPlanService = new UserPlanService(
            serviceProviderMock.Object,
            planRepoMock.Object,
            userRepoMock.Object,
            requestContextMock.Object,
            loggerMock.Object,
            userPlanPolicy.Object,
            expirationPolicy.Object,
            userPlanRepoMock.Object
        );
    }

    [Fact]
    public async Task Purchase_ShouldAddNewUserPlan_WhenValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        requestContextMock.Setup(repo => repo.GetUserId()).Returns(userId);
        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            Title = PlanTitle.Premium,
            Description = "",
            Price = 100,
            MaxTasks = 20,
            DurationDays = 30,
            IsActive = true
        };
        var user = new User
        {
            Id = userId,
            Email = "test@test.com",
            UserPlans = new List<UserPlan>()
        };
        var purchased = new UserPlan
        {
            UserId = userId,
            PlanId = plan.Id,
            IsActive = true,
            Plan = plan
        };

        userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<Expression<Func<User, object>>[]>())).ReturnsAsync(user);

        planRepoMock.Setup(r => r.GetByIdAsync(plan.Id)).ReturnsAsync(plan);
        userPlanRepoMock.Setup(r => r.AddAsync(It.IsAny<UserPlan>())).ReturnsAsync((UserPlan up) =>
        {
            up.Plan = plan; // set plan
            return up;
        });

        var result = await userPlanService.Purchase(plan.Id);

        result.Should().NotBeNull();
        result.PlanId.Should().Be(plan.Id);
        result.IsActive.Should().BeTrue();
        result.Plan.Should().NotBeNull();
        result.Plan.Title.Should().Be(plan.Title.ToString());
        userPlanRepoMock.Verify(r => r.AddAsync(It.IsAny<UserPlan>()), Times.Once);
    }
    [Fact]
    public async Task Purchase_ShouldThrowBusinessRuleException_WhenUserAlreadyHasActivePlan()
    {
        var userId = Guid.NewGuid();
        requestContextMock.Setup(repo => repo.GetUserId()).Returns(userId);
        var planId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@test.com",
            UserPlans = new List<UserPlan>
            {
                new UserPlan
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    PlanId = planId,
                    IsActive = true,
                    PurchasedAt = DateTime.UtcNow.AddDays(-1),
                    ExpiredAt = DateTime.UtcNow.AddDays(5)
                }
            }
        };

        userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<Expression<Func<User, object>>[]>())).ReturnsAsync(user);
        userPlanPolicy.Setup(up => up.Validate(userId)).ThrowsAsync(new BusinessRuleException("You already have a active plan, please deactive it and then purchase new one."));
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => userPlanService.Purchase(planId));

        Assert.Contains("You already have a active plan", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
    [Fact]
    public async Task Purchase_ShouldThrowNotFoundException_WhenPlanNotFound()
    {
        var userId = Guid.NewGuid();
        requestContextMock.Setup(repo => repo.GetUserId()).Returns(userId);
        var planId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@test.com",
            UserPlans = new List<UserPlan>()
        };

        userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<Expression<Func<User, object>>[]>())).ReturnsAsync(user);

        planRepoMock.Setup(r => r.GetByIdAsync(planId)).ReturnsAsync((Plan?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => userPlanService.Purchase(planId));

        Assert.Contains("Plan not found.", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Purchase_ShouldThrowNotFoundException_WhenUserNotFound()
    {
        var userId = Guid.NewGuid();
        requestContextMock.Setup(repo => repo.GetUserId()).Returns(userId);
        var planId = Guid.NewGuid();
        userPlanPolicy.Setup(up => up.Validate(userId)).ThrowsAsync(new NotFoundException("User"));
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => userPlanService.Purchase(planId));

        Assert.Contains("User not found.", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetUserPlanById_ShouldReturnUserPlanResponse_WhenPlanExists()
    {
        var userPlanId = Guid.NewGuid();
        var plan = new Plan { Id = Guid.NewGuid(), Title = PlanTitle.Premium, Price = 100, MaxTasks = 20 };
        var user = new User { Id = Guid.NewGuid(), Email = "test@test.com" };
        var userPlan = new UserPlan
        {
            Id = userPlanId,
            UserId = user.Id,
            PlanId = plan.Id,
            User = user,
            Plan = plan,
            IsActive = true,
            PurchasedAt = DateTime.UtcNow,
            ExpiredAt = DateTime.UtcNow.AddDays(30)
        };

        userPlanRepoMock.Setup(r => r.GetByIdAsync(userPlanId, It.IsAny<Expression<Func<UserPlan, object>>[]>())).ReturnsAsync(userPlan);

        var result = await userPlanService.GetUserPlanById(userPlanId);

        result.Should().NotBeNull();
        result.Id.Should().Be(userPlanId);
        result.UserId.Should().Be(user.Id);
        result.PlanId.Should().Be(plan.Id);
        result.Plan.Should().NotBeNull();
        result.Plan.Title.Should().Be(plan.Title.ToString());
    }
    [Fact]
    public async Task GetUserPlanById_ShouldThrowNotFoundException_WhenPlanDoesNotExist()
    {
        var userPlanId = Guid.NewGuid();
        userPlanRepoMock.Setup(r => r.GetByIdAsync(userPlanId, It.IsAny<Expression<Func<UserPlan, object>>[]>())).ReturnsAsync((UserPlan?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => userPlanService.GetUserPlanById(userPlanId));

        Assert.Contains("Prchased plan", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
    [Fact]
    public async Task GetActiveUserPlanByUserId_ShouldReturnActivePlan_WhenUserHasActivePlan()
    {
        var userId = Guid.NewGuid();
        var plan = new Plan { Id = Guid.NewGuid(), Title = PlanTitle.Premium, MaxTasks = 20, Price = 100 };
        var user = new User
        {
            Id = userId,
            Email = "test@test.com",
            UserPlans = new List<UserPlan>
            {
                new UserPlan
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    PlanId = plan.Id,
                    Plan = plan,
                    IsActive = true,
                    PurchasedAt = DateTime.UtcNow.AddDays(-1),
                    ExpiredAt = DateTime.UtcNow.AddDays(29)
                }
            }
        };

        userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<Expression<Func<User, object>>[]>())).ReturnsAsync(user);

        var result = await userPlanService.GetActiveUserPlanByUserId(userId);

        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.PlanId.Should().Be(plan.Id);
        result.Plan.Should().NotBeNull();
        result.Plan.Title.Should().Be(plan.Title.ToString());
    }
    [Fact]
    public async Task GetActiveUserPlanByUserId_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();

        userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<Expression<Func<User, object>>[]>())).ReturnsAsync((User?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => userPlanService.GetActiveUserPlanByUserId(userId));

        Assert.Contains("User not found", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
    [Fact]
    public async Task GetActiveUserPlanByUserId_ShouldThrowNotFoundException_WhenUserHasNoActivePlan()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@test.com",
            UserPlans = new List<UserPlan>
            {
                new UserPlan
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    IsActive = false,
                    ExpiredAt = DateTime.UtcNow.AddDays(-1)
                }
            }
        };

        userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<Expression<Func<User, object>>[]>())).ReturnsAsync(user);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => userPlanService.GetActiveUserPlanByUserId(userId));

        Assert.Contains("User plan not found", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
    [Fact]
    public async Task GetAllUserPlansByUserId_ShouldReturnAllPlans_WhenUserHasPlans()
    {
        var userId = Guid.NewGuid();
        var plan1 = new Plan { Id = Guid.NewGuid(), Title = PlanTitle.Free, MaxTasks = 5, Price = 0 };
        var plan2 = new Plan { Id = Guid.NewGuid(), Title = PlanTitle.Premium, MaxTasks = 20, Price = 100 };

        var user = new User
        {
            Id = userId,
            Email = "test@test.com",
            UserPlans = new List<UserPlan>
            {
                new UserPlan { Id = Guid.NewGuid(), UserId = userId, Plan = plan1, PlanId = plan1.Id },
                new UserPlan { Id = Guid.NewGuid(), UserId = userId, Plan = plan2, PlanId = plan2.Id }
            }
        };

        userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<Expression<Func<User, object>>[]>())).ReturnsAsync(user);

        var result = await userPlanService.GetAllUserPlansByUserId(userId);

        result.Should().NotBeNull();
        result.Count().Should().Be(2);
        result.Select(r => r.Plan!.Title).Should().Contain(new[] { plan1.Title.ToString(), plan2.Title.ToString() });
    }


    [Fact]
    public async Task GetAllUserPlansByUserId_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();

        userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<Expression<Func<User, object>>[]>())).ReturnsAsync((User?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => userPlanService.GetAllUserPlansByUserId(userId));

        Assert.Contains("User not found.", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetAllUserPlansByUserId_ShouldReturnEmptyList_WhenUserHasNoPlans()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@test.com",
            UserPlans = new List<UserPlan>()
        };

        userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<Expression<Func<User, object>>[]>())).ReturnsAsync(user);

        var result = await userPlanService.GetAllUserPlansByUserId(userId);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllUsersByPlanId_ShouldReturnAllUsers_WhenPlanHasUsers()
    {
        var planId = Guid.NewGuid();
        var user1 = new User { Id = Guid.NewGuid(), Email = "user1@test.com" };
        var user2 = new User { Id = Guid.NewGuid(), Email = "user2@test.com" };

        var plan = new Plan
        {
            Id = planId,
            Title = PlanTitle.Premium,
            UserPlans = new List<UserPlan>
            {
                new UserPlan { User = user1, UserId = user1.Id, PlanId = planId },
                new UserPlan { User = user2, UserId = user2.Id, PlanId = planId }
            }
        };

        planRepoMock.Setup(r => r.GetByIdAsync(planId, It.IsAny<Expression<Func<Plan, object>>[]>())).ReturnsAsync(plan);

        var result = await userPlanService.GetAllUsersByPlanId(planId);

        result.Should().NotBeNull();
        result.Count().Should().Be(2);
        result.Select(u => u.Email).Should().Contain(new[] { "user1@test.com", "user2@test.com" });
    }
    [Fact]
    public async Task GetAllUsersByPlanId_ShouldThrowNotFoundException_WhenPlanDoesNotExist()
    {
        var planId = Guid.NewGuid();

        planRepoMock.Setup(r => r.GetByIdAsync(planId, It.IsAny<Expression<Func<Plan, object>>[]>())).ReturnsAsync((Plan?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => userPlanService.GetAllUsersByPlanId(planId));

        Assert.Contains("Plan", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
    [Fact]
    public async Task GetAllUsersByPlanId_ShouldReturnEmptyList_WhenPlanHasNoUsers()
    {
        var planId = Guid.NewGuid();
        var plan = new Plan
        {
            Id = planId,
            Title = PlanTitle.Premium,
            UserPlans = new List<UserPlan>()
        };

        planRepoMock.Setup(r => r.GetByIdAsync(planId, It.IsAny<Expression<Func<Plan, object>>[]>())).ReturnsAsync(plan);

        var result = await userPlanService.GetAllUsersByPlanId(planId);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
    [Fact]
    public async Task DeactivateAsync_ShouldDeactivateUserPlan_WhenValid()
    {
        var userPlanId = Guid.NewGuid();
        var plan = new UserPlan
        {
            Id = userPlanId,
            UserId = Guid.NewGuid(),
            IsActive = true,
            ExpiredAt = DateTime.UtcNow.AddDays(5)
        };

        userPlanRepoMock.Setup(r => r.GetByIdAsync(userPlanId)).ReturnsAsync(plan);
        expirationPolicy.Setup(xp => xp.Validate(It.IsAny<UserPlan>())).Returns(Task.CompletedTask);
        userPlanRepoMock.Setup(r => r.UpdateAsync(It.IsAny<UserPlan>())).Returns(Task.CompletedTask);

        var result = await userPlanService.DeactivateAsync(userPlanId);


        result.Should().NotBeNull();
        result.Massage.Should().Contain("deactivated");
        userPlanRepoMock.Verify(r => r.UpdateAsync(It.Is<UserPlan>(up => up.Id == userPlanId && up.IsActive == false)), Times.Once);
    }
    [Fact]
    public async Task DeactivateAsync_ShouldThrowNotFoundException_WhenUserPlanDoesNotExist()
    {
        var userPlanId = Guid.NewGuid();

        userPlanRepoMock.Setup(r => r.GetByIdAsync(userPlanId)).ReturnsAsync((UserPlan?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => userPlanService.DeactivateAsync(userPlanId));

        Assert.Contains("Prchased plan", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeactivateAsync_ShouldThrowBusinessRuleException_WhenPlanAlreadyDeactivated()
    {
         var userId = Guid.NewGuid();
        requestContextMock.Setup(repo => repo.GetUserId()).Returns(userId);
        var userPlanId = Guid.NewGuid();
        var expiredDate = DateTime.UtcNow.AddDays(-1);
        var plan = new UserPlan
        {
            Id = userPlanId,
            UserId = Guid.NewGuid(),
            IsActive = false,
            ExpiredAt = true ? expiredDate : DateTime.UtcNow.AddDays(5)
        };

        userPlanRepoMock.Setup(r => r.GetByIdAsync(userPlanId)).ReturnsAsync(plan);
        expirationPolicy.Setup(xp => xp.Validate(It.IsAny<UserPlan>())).ThrowsAsync(new BusinessRuleException("The Plan has expired."));
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => userPlanService.DeactivateAsync(userPlanId));

        Assert.Contains("The Plan has expired.", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

}
