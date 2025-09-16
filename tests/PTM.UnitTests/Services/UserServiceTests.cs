using System;
using FluentAssertions;
using Moq;
using PTM.Application.Exceptions;
using PTM.Application.Interfaces;
using PTM.Application.Services;
using PTM.Contracts.Requests;
using PTM.Domain.Models;

namespace PTM.UnitTests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> userRepositoryMock;
    private readonly Mock<IServiceProvider> serviceProviderMock;
    private readonly UserService userService;
    public UserServiceTests()
    {
        userRepositoryMock = new Mock<IUserRepository>();
        serviceProviderMock = new Mock<IServiceProvider>();
        userService = new UserService(userRepositoryMock.Object, serviceProviderMock.Object);
    }
    [Fact]
    public async Task GetAllUsers_ShouldBeSuccessfull()
    {
        var users = new List<User>
        {
            new User{
                Id = Guid.NewGuid(),
                Username="username",
                Email="email@email.com",
                Password="123"
            },
            new User{
                Id = Guid.NewGuid(),
                Username="username2",
                Email="email2@email.com",
                Password="123"
            },
        };
        userRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(users);
        var result = await userService.GetAllAsync();

        result.Should().HaveCount(2);
        result.Should().ContainSingle(user => user.Username == "username");
        result.Should().ContainSingle(user => user.Username == "username2");
    }
    [Fact]
    public async Task GetUserById_ShouldReturnUser_WhenUserExists()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "username",
            Email = "email@email.com",
            Password = "123"
        };
        userRepositoryMock.Setup(repo => repo.GetByIdAsync(user.Id)).ReturnsAsync(user);
        var result = await userService.GetByIdAsync(user.Id);

        result.Should().NotBe(null);
        result.Username.Should().Be("username");

    }
    [Fact]
    public async Task GetUserById_ShouldThrowNotFoundException_WhenUserNotExists()
    {
        var userId = Guid.NewGuid();
        userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((User?)null);
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => userService.GetByIdAsync(userId));

        exception.Message.Should().Contain("not found");
    }
    [Fact]
    public async Task UpdateUser_ShouldReturnUser_WhenUserExists()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "username",
            Email = "email@email.com",
            Password = "123"
        };
        var updateReq = new UserUpdateRequest
        {
            Id = user.Id,
            Username = "username2",
            Email = "email@email.com",
        };
        userRepositoryMock.Setup(repo => repo.GetByIdAsync(user.Id)).ReturnsAsync(user);
        userRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        var result = await userService.UpdateAsync(user.Id, updateReq);

        result.Should().NotBe(null);
        result.Username.Should().Be("username2");

    }
    [Fact]
    public async Task UpdateUser_ShouldThrowNotFoundException_WhenUserNotExists()
    {
        var userId = Guid.NewGuid();
        var updateReq = new UserUpdateRequest
        {
            Id = userId,
            Username = "username2",
            Email = "email@email.com",
        };
        userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((User?)null);
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => userService.UpdateAsync(userId, updateReq));

        exception.Message.Should().Contain("not found");
    }
}
