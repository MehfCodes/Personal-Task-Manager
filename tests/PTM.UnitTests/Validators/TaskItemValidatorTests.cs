using System;
using FluentAssertions;
using PTM.Application.Validation.Validators.TaskItem;
using PTM.Contracts.Requests;

namespace PTM.UnitTests.Validators;

public class TaskItemValidatorTests
{
    private readonly TaskItemValidationRules _validator = new();

  
    [Fact]
    public async Task ValidateAsync_ShouldBeValid_WhenModelIsValid()
    {
        // Given
        var model = new TaskItemRequest
        {
            Title = "My task",
            Description = "Some description",
            Priority = "High",
            Status = "Todo"
        };

        // When
        var result = await _validator.ValidateAsync(model);

        // Then
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_ShouldnotValid_WhenTitleIsEmpty()
    {
        var model = new TaskItemRequest
        {
            Title = "",
            Description = "desc",
            Priority = "High",
            Status = "Todo"
        };

        var result = await _validator.ValidateAsync(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title" && e.ErrorMessage.Contains("Title"));
    }

    [Fact]
    public async Task ValidateAsync_ShouldBeValid_WhenDescriptionIsNull()
    {
        var model = new TaskItemRequest
        {
            Title = "Task",
            Description = null,
            Priority = "High",
            Status = "Todo"
        };

        var result = await _validator.ValidateAsync(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }

    [Fact]
    public async Task ValidateAsync_ShouldBeValid_WhenPriorityIsInvalid()
    {
        var model = new TaskItemRequest
        {
            Title = "Task",
            Description = "desc",
            Priority = "InvalidPriority",
            Status = "Todo"
        };

        var result = await _validator.ValidateAsync(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Priority");
    }

    [Fact]
    public async Task ValidateAsync_ShouldBeValid_WhenStatusIsInvalid()
    {
        var model = new TaskItemRequest
        {
            Title = "Task",
            Description = "desc",
            Priority = "High",
            Status = "NotAStatus"
        };

        var result = await _validator.ValidateAsync(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Status");
    }

}
