using System;
using System.Threading.Tasks;
using FluentAssertions;
using PTM.Application.Validation.Validators.Plan;
using PTM.Contracts.Requests;
using Xunit.Abstractions;

namespace PTM.UnitTests.Validators;

public class CreatePlanRequestValidatorTests
{
    private readonly PlanRequestValidator validator = new();

    public CreatePlanRequestValidatorTests()
    {
    }
    [Fact]
    public async Task ValidateAsync_ShouldBeValid_WhenModelIsCorrect()
    {
        // Given
        PlanRequest model = new PlanRequest
        {
            Title = "Free",
            Description = "des",
            Price = 0,
            MaxTasks = 5,
            DurationDays = 7,
            IsActive = true
        };

        // When
        var res = await validator.ValidateAsync(model);

        // Then
        res.Should().NotBeNull();
        res.IsValid.Should().BeTrue();
    }
    [Fact]
    public async Task ValidateAsync_ShouldBeValid_WhenModelIsCorrectForPremiumPlan()
    {
        // Given
        PlanRequest model = new PlanRequest
        {
            Title = "Premium",
            Description = "des",
            Price = 10,
            MaxTasks = 15,
            DurationDays = 30,
            IsActive = true
        };

        // When
        var res = await validator.ValidateAsync(model);

        // Then
        res.Should().NotBeNull();
        res.IsValid.Should().BeTrue();
    }
    [Fact]
    public async Task ValidateAsync_ShouldBeValid_WhenModelIsCorrectForBusinessPlan()
    {
        // Given
        PlanRequest model = new PlanRequest
        {
            Title = "Business",
            Description = "des",
            Price = 100,
            MaxTasks = -1,
            DurationDays = 365,
            IsActive = true
        };

        // When
        var res = await validator.ValidateAsync(model);

        // Then
        res.Should().NotBeNull();
        res.IsValid.Should().BeTrue();
    }
    [Fact]
    public async Task ValidateAsync_ShouldNotValid_WhenTitleIsEmpty()
    {
        // Given
        PlanRequest model = new PlanRequest
        {
            Title = "",
            Description = "des",
            Price = 0,
            MaxTasks = 5,
            DurationDays = 7,
            IsActive = true
        };

        // When
        var res = await validator.ValidateAsync(model);

        // Then
        res.Should().NotBeNull();
        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e => e.PropertyName == "Title");
    }
    [Fact]
    public async Task ValidateAsync_ShouldNotValid_WhenPriceIsMoreThanZeroForFreePLan()
    {
        // Given
        PlanRequest model = new PlanRequest
        {
            Title = "Free",
            Description = "des",
            MaxTasks = 5,
            Price = 10,
            DurationDays = 7,
            IsActive = true
        };

        // When
        var res = await validator.ValidateAsync(model);

        // Then
        res.Should().NotBeNull();
        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e => e.PropertyName == "Price");
    }
    [Fact]
    public async Task ValidateAsync_ShouldNotValid_WhenPriceIsZeroForNotFreePLan()
    {
        // Given
        PlanRequest model = new PlanRequest
        {
            Title = "Premium",
            Description = "des",
            MaxTasks = 15,
            Price = 0,
            DurationDays = 30,
            IsActive = true
        };

        // When
        var res = await validator.ValidateAsync(model);

        // Then
        res.Should().NotBeNull();
        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e => e.PropertyName == "Price");
    }
    [Fact]
    public async Task ValidateAsync_ShouldNotValid_WhenMaxTasksIsNegative()
    {
        // Given
        PlanRequest model = new PlanRequest
        {
            Title = "Free",
            Description = "des",
            Price = 0,
            MaxTasks = -5,
            DurationDays = 7,
            IsActive = true
        };

        // When
        var res = await validator.ValidateAsync(model);

        // Then
        res.Should().NotBeNull();
        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e => e.PropertyName == "MaxTasks");
    }
    [Fact]
    public async Task ValidateAsync_ShouldNotValid_WhenMaxTasksIsNotBetween1To10ForFreePlan()
    {
        // Given
        PlanRequest model = new PlanRequest
        {
            Title = "Free",
            Description = "des",
            Price = 0,
            MaxTasks = 15,
            DurationDays = 7,
            IsActive = true
        };

        // When
        var res = await validator.ValidateAsync(model);

        // Then
        res.Should().NotBeNull();
        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e => e.PropertyName == "MaxTasks");
    }
    [Fact]
    public async Task ValidateAsync_ShouldNotValid_WhenMaxTasksIsNotGreaterThan10ForPremiumPlan()
    {
        // Given
        PlanRequest model = new PlanRequest
        {
            Title = "Premium",
            Description = "des",
            Price = 10,
            MaxTasks = 3,
            DurationDays = 30,
            IsActive = true
        };

        // When
        var res = await validator.ValidateAsync(model);

        // Then
        res.Should().NotBeNull();
        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e => e.PropertyName == "MaxTasks");
    }
    [Fact]
    public async Task ValidateAsync_ShouldNotValid_WhenMaxTasksIsNotInfinityForBusinessPlan()
    {
        // Given
        PlanRequest model = new PlanRequest
        {
            Title = "business",
            Description = "des",
            Price = 10,
            MaxTasks = 1,
            DurationDays = 365,
            IsActive = true
        };

        // When
        var res = await validator.ValidateAsync(model);

        // Then
        res.Should().NotBeNull();
        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e => e.PropertyName == "MaxTasks");
    }
    [Fact]
    public async Task ValidateAsync_ShouldNotValid_WhenDurationDaysIsNotNaturalNumber()
    {
        // Given
        PlanRequest model = new PlanRequest
        {
            Title = "free",
            Description = "des",
            Price = 10,
            MaxTasks = 4,
            DurationDays = 0,
            IsActive = true
        };

        // When
        var res = await validator.ValidateAsync(model);

        // Then
        res.Should().NotBeNull();
        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e => e.PropertyName == "DurationDays");
    }
    [Fact]
    public async Task ValidateAsync_ShouldNotValid_WhenDurationDaysIsNotCorrectForFreePlan()
    {
        // Given
        PlanRequest model = new PlanRequest
        {
            Title = "free",
            Description = "des",
            Price = 10,
            MaxTasks = 4,
            DurationDays = 14,
            IsActive = true
        };

        // When
        var res = await validator.ValidateAsync(model);

        // Then
        res.Should().NotBeNull();
        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e => e.PropertyName == "DurationDays");
    }
    [Fact]
    public async Task ValidateAsync_ShouldNotValid_WhenDurationDaysIsNotCorrectForPremiumPlan()
    {
        // Given
        PlanRequest model = new PlanRequest
        {
            Title = "Premium",
            Description = "des",
            Price = 10,
            MaxTasks = 14,
            DurationDays = 20,
            IsActive = true
        };

        // When
        var res = await validator.ValidateAsync(model);

        // Then
        res.Should().NotBeNull();
        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e => e.PropertyName == "DurationDays");
    }
    [Fact]
    public async Task ValidateAsync_ShouldNotValid_WhenDurationDaysIsNotCorrectForBusinessPlan()
    {
        // Given
        PlanRequest model = new PlanRequest
        {
            Title = "business",
            Description = "des",
            Price = 10,
            MaxTasks = -1,
            DurationDays = 120,
            IsActive = true
        };

        // When
        var res = await validator.ValidateAsync(model);

        // Then
        res.Should().NotBeNull();
        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e => e.PropertyName == "DurationDays");
    }
}
