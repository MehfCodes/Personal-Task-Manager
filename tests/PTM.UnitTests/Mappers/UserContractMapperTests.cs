
using FluentAssertions;
using PTM.Application.Mappers;
using PTM.Contracts.Requests;
using PTM.Domain.Models;

namespace PTM.UnitTests.Mappers;

public class UserContractMapperTests
{
    [Fact]
    public void MapToUser_Should_Map_RegisterRequest_To_User_With_HashedPassword()
    {
        // Arrange
        var request = new UserRegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            PhoneNumber = "123456789",
            Password = "PlainPassword123"
        };

        // Act
        var result = request.MapToUser();

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be(request.Username);
        result.Email.Should().Be(request.Email);
        result.PhoneNumber.Should().Be(request.PhoneNumber);
        result.Password.Should().NotBe(request.Password);
        BCrypt.Net.BCrypt.Verify(request.Password, result.Password).Should().BeTrue();
    }

    [Fact]
    public void MapToUser_Should_Update_Existing_User()
    {
        // Arrange
        var request = new UserUpdateRequest
        {
            Username = "updatedUser",
            Email = "updated@example.com",
            PhoneNumber = "987654321"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "oldUser",
            Email = "old@example.com",
            PhoneNumber = "111111111",
            Password = "hashedpassword"
        };

        // Act
        var result = request.MapToUser(user);

        // Assert
        result.Username.Should().Be(request.Username);
        result.Email.Should().Be(request.Email);
        result.PhoneNumber.Should().Be(request.PhoneNumber);
        result.Password.Should().Be("hashedpassword");
    }

    [Fact]
    public void MapToUserResponse_Should_Map_User_To_Response_With_Plans()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            PhoneNumber = "123456789",
            UserPlans = new List<UserPlan>
            {
                new UserPlan { PlanId = Guid.NewGuid(), Plan = new Plan { Title = PlanTitle.Free } },
                new UserPlan { PlanId = Guid.NewGuid(), Plan = new Plan { Title = PlanTitle.Premium } }
            }
        };

        // Act
        var result = user.MapToUserResponse();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.Username.Should().Be(user.Username);
        result.Email.Should().Be(user.Email);
        result.PhoneNumber.Should().Be(user.PhoneNumber);

        result.Plans.Should().HaveCount(2);
        result.Plans.Should().ContainSingle(p => p.Title == "Free");
        result.Plans.Should().ContainSingle(p => p.Title == "Premium");
    }

    [Fact]
    public void MapToUsersResponse_Should_Map_List_Of_Users()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), Username = "User1", Email = "u1@test.com", PhoneNumber = "111", UserPlans = new List<UserPlan>() },
            new User { Id = Guid.NewGuid(), Username = "User2", Email = "u2@test.com", PhoneNumber = "222", UserPlans = new List<UserPlan>() }
        };

        // Act
        var result = users.MapToUsersResponse();

        // Assert
        result.Should().NotBeNull().And.HaveCount(2);
        result.Should().ContainSingle(r => r.Username == "User1");
        result.Should().ContainSingle(r => r.Username == "User2");
    }
}
