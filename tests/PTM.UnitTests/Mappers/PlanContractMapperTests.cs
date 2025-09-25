
using FluentAssertions;
using PTM.Application.Mappers;
using PTM.Contracts.Requests;
using PTM.Domain.Models;

namespace PTM.UnitTests.Mappers;

public class PlanContractMapperTests
{
    [Fact]
    public void MapToPlan_Should_Map_All_Premiumperties_Correctly()
    {
        // Given
        var request = new PlanRequest
        {
            Title = "Premium",
            Description = "Premium plan",
            Price = 100,
            MaxTasks = 50,
            DurationDays = 30,
            IsActive = true
        };

        // When
        var result = request.MapToPlan();

        // Then
        result.Should().NotBeNull();
        result.Title.Should().Be(PlanTitle.Premium);
        result.Description.Should().Be(request.Description);
        result.Price.Should().Be(request.Price);
        result.MaxTasks.Should().Be(request.MaxTasks);
        result.DurationDays.Should().Be(request.DurationDays);
        result.IsActive.Should().Be(request.IsActive);
    }

    
    [Fact]
public void MapToPlan_Update_Should_Map_All_Premiumperties_Correctly()
{
    // Given
    var existingPlan = new Plan { Id = Guid.NewGuid(), Title = PlanTitle.Free };

    var updateRequest = new PlanUpdateRequest
    {
        Id = Guid.NewGuid(),
        Title = "Premium",
        Description = "Updated desc",
        Price = 200,
        MaxTasks = 100,
        DurationDays = 60,
        IsActive = true
    };

    // When
    var result = updateRequest.MapToPlan(existingPlan);

    // Then
    result.Id.Should().Be(updateRequest.Id);
    result.Title.Should().Be(PlanTitle.Premium);
    result.Description.Should().Be(updateRequest.Description);
    result.Price.Should().Be(updateRequest.Price);
    result.MaxTasks.Should().Be(updateRequest.MaxTasks);
    result.DurationDays.Should().Be(updateRequest.DurationDays);
    result.IsActive.Should().Be(updateRequest.IsActive);
}

    [Fact]
    public void MapToPlan_Update_Should_DefaultToEmptyDescription_When_Null()
    {
        // Given
        var existingPlan = new Plan { Id = Guid.NewGuid() };

        var updateRequest = new PlanUpdateRequest
        {
            Id = Guid.NewGuid(),
            Title = "Free",
            Description = null,
            Price = 0,
            MaxTasks = 10,
            DurationDays = 5,
            IsActive = false
        };

        // When
        var result = updateRequest.MapToPlan(existingPlan);

        // Then
        result.Description.Should().BeEmpty();
    }
[Fact]
public void MapToPlanResponse_Should_Map_All_Premiumperties_Correctly()
{
    // Given
    var plan = new Plan
    {
        Id = Guid.NewGuid(),
        Title = PlanTitle.Premium,
        Description = "Premium plan",
        Price = 100,
        MaxTasks = 20,
        DurationDays = 30,
        IsActive = true
    };

    // When
    var result = plan.MapToPlanResponse();

    // Then
    result.Should().NotBeNull();
    result.Id.Should().Be(plan.Id);
    result.Title.Should().Be(plan.Title.ToString());
    result.Description.Should().Be(plan.Description);
    result.Price.Should().Be(plan.Price);
    result.MaxTasks.Should().Be(plan.MaxTasks);
    result.DurationDays.Should().Be(plan.DurationDays);
    result.IsActive.Should().Be(plan.IsActive);
}
[Fact]
public void MapToPlansResponse_Should_Map_List_Correctly()
{
    // Given
    var plans = new List<Plan>
    {
        new Plan { Id = Guid.NewGuid(), Title = PlanTitle.Free, Description = "Free", Price = 0, MaxTasks = 5, DurationDays = 7, IsActive = true },
        new Plan { Id = Guid.NewGuid(), Title = PlanTitle.Premium, Description = "Premium", Price = 100, MaxTasks = 50, DurationDays = 30, IsActive = true }
    };

    // When
    var result = plans.MapToPlansResponse().ToList();

    // Then
    result.Should().NotBeNull();
    result.Should().HaveCount(2);
    result[0].Title.Should().Be(PlanTitle.Free.ToString());
    result[1].Title.Should().Be(PlanTitle.Premium.ToString());
}


}

