using System;
using FluentAssertions;
using PTM.Application.Mappers;
using PTM.Contracts.Requests;
using PTM.Domain.Models;

namespace PTM.UnitTests.Mappers;

public class TaskItemContractMapperTests
{
    [Fact]
    public void MapToTaskItem_Should_Map_Request_To_Entity()
    {
        // Arrange
        var request = new TaskItemRequest
        {
            Title = "Test Title",
            Description = "Test Description",
            Status = "Todo",
            Priority = "High"
        };

        // Act
        var result = request.MapToTaskItem();

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(request.Title);
        result.Description.Should().Be(request.Description);
        result.Status.Should().Be(Status.Todo);
        result.Priority.Should().Be(Priority.High);
    }

    [Fact]
    public void MapToTaskItem_Should_Update_Entity()
    {
        // Arrange
        var request = new TaskItemUpdateRequest
        {
            Title = "Updated Title",
            Description = "Updated Description",
            Status = "Done",
            Priority = "Low"
        };
        var entity = new TaskItem
        {
            Title = "Old Title",
            Description = "Old Description",
            Status = Status.Todo,
            Priority = Priority.High
        };

        // Act
        var result = request.MapToTaskItem(entity);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(request.Title);
        result.Description.Should().Be(request.Description);
        result.Status.Should().Be(Status.Done);
        result.Priority.Should().Be(Priority.Low);
    }

    [Fact]
    public void MapToTaskItemResponse_Should_Map_Entity_To_Response()
    {
        // Arrange
        var entity = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Test Title",
            Description = "Test Description",
            Status = Status.InProgress,
            Priority = Priority.Mid
        };

        // Act
        var result = entity.MapToTaskItemResponse();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(entity.Id);
        result.Title.Should().Be(entity.Title);
        result.Description.Should().Be(entity.Description);
        result.Status.Should().Be("InProgress");
        result.Priority.Should().Be("Mid");
    }

    [Fact]
    public void MapToTaskItemsResponse_Should_Map_List_Of_Entities()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        // Arrange
        var entities = new List<TaskItem>
        {
            new TaskItem { Id = id1, Title = "Task1", Description = "Desc1", Status = Status.Todo, Priority = Priority.High },
            new TaskItem { Id = id2, Title = "Task2", Description = "Desc2", Status = Status.Done, Priority = Priority.Low }
        };

        // Act
        var result = entities.MapToTaskItemsResponse();

        // Assert
        result.Should().NotBeNull().And.HaveCount(2);
        result.Should().ContainSingle(r => r.Id == id1 && r.Title == "Task1");
        result.Should().ContainSingle(r => r.Id == id2 && r.Status == "Done");
    }
}
