
using FluentAssertions;
using PTM.Application.Mappers;
using PTM.Domain.Models;

public class UserPlanContractMapperTests
{
    [Fact]
    public void MapToUserPlanResponse_Should_Map_Properties_Correctly()
    {
        // Arrange
        var userPlan = new UserPlan
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            PlanId = Guid.NewGuid(),
            IsActive = true,
            PurchasedAt = DateTime.UtcNow,
            ExpiredAt = DateTime.UtcNow.AddDays(30)
        };

        // Act
        var result = userPlan.MapToUserPlanResponse();

        // Assert
        result.Id.Should().Be(userPlan.Id);
        result.UserId.Should().Be(userPlan.UserId);
        result.PlanId.Should().Be(userPlan.PlanId);
        result.IsActive.Should().BeTrue();
        result.PurchasedAt.Should().Be(userPlan.PurchasedAt);
        result.ExpiredAt.Should().Be(userPlan.ExpiredAt);
        result.Plan.Should().BeNull();
        result.User.Should().BeNull();
    }

    [Fact]
    public void MapToUserPlanDetailResponse_Should_Map_Properties_With_Plan_And_User()
    {
        // Arrange
        var plan = new Plan { Id = Guid.NewGuid(), Title = PlanTitle.Premium, Price = 100 };
        var user = new User { Id = Guid.NewGuid(), Username = "testuser", Email = "test@example.com" };
        var userPlan = new UserPlan
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            PlanId = plan.Id,
            IsActive = true,
            PurchasedAt = DateTime.UtcNow,
            ExpiredAt = DateTime.UtcNow.AddDays(30),
            Plan = plan,
            User = user
        };

        // Act
        var result = userPlan.MapToUserPlanDetailResponse();

        // Assert
        result.Id.Should().Be(userPlan.Id);
        result.UserId.Should().Be(user.Id);
        result.PlanId.Should().Be(plan.Id);
        result.IsActive.Should().BeTrue();
        result.Plan.Should().NotBeNull();
        result.Plan.Title.Should().Be(plan.Title.ToString());
        result.User.Should().NotBeNull();
        result.User.Username.Should().Be(user.Username);
    }

    [Fact]
    public void MapToUserPlanWithPlanDetailResponse_Should_Map_Properties_With_Plan_Only()
    {
        // Arrange
        var plan = new Plan { Id = Guid.NewGuid(), Title = PlanTitle.Free, Price = 0 };
        var userPlan = new UserPlan
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            PlanId = plan.Id,
            IsActive = false,
            PurchasedAt = DateTime.UtcNow,
            ExpiredAt = DateTime.UtcNow.AddDays(7),
            Plan = plan
        };

        // Act
        var result = userPlan.MapToUserPlanWithPlanDetailResponse();

        // Assert
        result.Plan.Should().NotBeNull();
        result.Plan.Title.Should().Be(plan.Title.ToString());
        result.User.Should().BeNull();
    }

    [Fact]
    public void MapToUserPlansResponse_Should_Map_List_Of_UserPlans()
    {
        // Arrange
        var plan1 = new Plan { Id = Guid.NewGuid(), Title = PlanTitle.Free };
        var plan2 = new Plan { Id = Guid.NewGuid(), Title = PlanTitle.Premium };
        var userPlans = new List<UserPlan>
        {
            new UserPlan { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), PlanId = plan1.Id, Plan = plan1 },
            new UserPlan { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), PlanId = plan2.Id, Plan = plan2 }
        };

        // Act
        var result = userPlans.MapToUserPlansResponse();

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainSingle(r => r.Plan!.Title == "Free");
        result.Should().ContainSingle(r => r.Plan!.Title == "Premium");
    }
}
