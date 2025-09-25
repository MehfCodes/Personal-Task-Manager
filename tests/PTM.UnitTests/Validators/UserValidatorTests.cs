using System;
using FluentAssertions;
using PTM.Application.Validation.Validators.User;
using PTM.Contracts.Requests;

namespace PTM.UnitTests.Validators;

public class UserValidatorTests
{
    private readonly UserRequestValidator validator = new();
    public UserValidatorTests()
    {

    }
    [Fact]
    public async Task ValidateAsync_ShouldBeValid_WhenModelIsCorrect()
    {
        // Given
        var model = new UserRegisterRequest
        {
            Username = "Mehf",
            Email = "email@email.com",
            PhoneNumber = "09123456789",
            Password = "@Password123"
        };

        // When
        var res = await validator.ValidateAsync(model);

        // Then
        res.Should().NotBeNull();
        res.IsValid.Should().BeTrue();
    }
    [Fact]
    public async Task ValidateAsync_ShouldNotValid_WhenPropertyIsNullOrEmpty()
    {
        // Given
        var model = new UserRegisterRequest
        {
            Username = "",
            Email = "email@email.com",
            PhoneNumber = "09123456789",
            Password = "@Password123"
        };

        // When
        var res = await validator.ValidateAsync(model);

        // Then
        res.Should().NotBeNull();
        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e => e.PropertyName == "Username");
    }
    [Fact]
    public async Task ValidateAsync_ShouldNotValid_WhenUsernameMinimumLengthIsNotCorrect()
    {
        // Given
        var model = new UserRegisterRequest
        {
            Username = "abc",
            Email = "email@email.com",
            PhoneNumber = "09123456789",
            Password = "@Password123"
        };

        // When
        var res = await validator.ValidateAsync(model);

        // Then
        res.Should().NotBeNull();
        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e => e.PropertyName == "Username");
    }
    [Fact]
    public async Task ValidateAsync_ShouldNotValid_WhenEmailFormatIsNotCorrect()
    {
        // Given
        var model = new UserRegisterRequest
        {
            Username = "Mehf",
            Email = "email.com",
            PhoneNumber = "09123456789",
            Password = "@Password123"
        };

        // When
        var res = await validator.ValidateAsync(model);

        // Then
        res.Should().NotBeNull();
        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e => e.PropertyName == "Email");
    }
    [Fact]
    public async Task ValidateAsync_ShouldNotValid_WhenPhoneNumberFormatIsNotCorrect()
    {
        // Given
        var model = new UserRegisterRequest
        {
            Username = "Mehf",
            Email = "email@email.com",
            PhoneNumber = "1234",
            Password = "@Password123"
        };

        // When
        var res = await validator.ValidateAsync(model);

        // Then
        res.Should().NotBeNull();
        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e => e.PropertyName == "PhoneNumber");
    }
    [Fact]
    public async Task ValidateAsync_ShouldNotValid_WhenPasswordLengthIsNotCorrect()
    {
        // Given
        var model = new UserRegisterRequest
        {
            Username = "Mehf",
            Email = "email@email.com",
            PhoneNumber = "09123456789",
            Password = "@Pass"
        };

        // When
        var res = await validator.ValidateAsync(model);

        // Then
        res.Should().NotBeNull();
        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e => e.PropertyName == "Password");
    }
    [Fact]
    public async Task ValidateAsync_ShouldNotValid_WhenThereIsNotUpperCaseLetterInPassword()
    {
        // Given
        var model = new UserRegisterRequest
        {
            Username = "Mehf",
            Email = "email@email.com",
            PhoneNumber = "09123456789",
            Password = "@password123"
        };

        // When
        var res = await validator.ValidateAsync(model);

        // Then
        res.Should().NotBeNull();
        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e => e.PropertyName == "Password");
    }
    [Fact]
    public async Task ValidateAsync_ShouldNotValid_WhenThereIsNotLowerCaseLetterInPassword()
    {
        // Given
        var model = new UserRegisterRequest
        {
            Username = "Mehf",
            Email = "email@email.com",
            PhoneNumber = "09123456789",
            Password = "@PASSWORD123"
        };

        // When
        var res = await validator.ValidateAsync(model);

        // Then
        res.Should().NotBeNull();
        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e => e.PropertyName == "Password");
    }
    [Fact]
    public async Task ValidateAsync_ShouldNotValid_WhenThereIsNotNumberInPassword()
    {
        // Given
        var model = new UserRegisterRequest
        {
            Username = "Mehf",
            Email = "email@email.com",
            PhoneNumber = "09123456789",
            Password = "@PASSword"
        };

        // When
        var res = await validator.ValidateAsync(model);

        // Then
        res.Should().NotBeNull();
        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e => e.PropertyName == "Password");
    }
    [Fact]
    public async Task ValidateAsync_ShouldNotValid_WhenThereIsNotSpecialCharacterInPassword()
    {
        // Given
        var model = new UserRegisterRequest
        {
            Username = "Mehf",
            Email = "email@email.com",
            PhoneNumber = "09123456789",
            Password = "PASSword123"
        };

        // When
        var res = await validator.ValidateAsync(model);

        // Then
        res.Should().NotBeNull();
        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e => e.PropertyName == "Password");
    }
    [Fact]
    public async Task ValidateAsync_ShouldNotValid_WhenPasswordContainSpace()
    {
        // Given
        var model = new UserRegisterRequest
        {
            Username = "Mehf",
            Email = "email@email.com",
            PhoneNumber = "09123456789",
            Password = "@ PASSword123"
        };

        // When
        var res = await validator.ValidateAsync(model);

        // Then
        res.Should().NotBeNull();
        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e => e.PropertyName == "Password");
    }
}
