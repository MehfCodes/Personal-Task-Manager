using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using PTM.Application.Interfaces.Authentication;
using PTM.Application.Interfaces.Repositories;
using PTM.Application.Interfaces.Services;
using PTM.Application.Services;
using PTM.Domain.Models;

namespace PTM.UnitTests.Services;

public class RefreshTokenServiceTests
{
    private readonly Mock<IRefreshTokenRepository> refreshTokenRepository = new();
    private readonly RefreshTokenService refreshTokenService;
    private readonly Mock<IRequestContext> requestContextMock = new();
    private readonly Mock<ITokenService> tokenServiceMock = new();
    private string ipAddress;
    private string userAgent;
    public RefreshTokenServiceTests()
    {
        requestContextMock.Setup(repo => repo.GetIpAddress()).Returns(ipAddress);
        requestContextMock.Setup(repo => repo.GetUserAgent()).Returns(userAgent);
        refreshTokenService = new RefreshTokenService(refreshTokenRepository.Object,
        tokenServiceMock.Object,
        requestContextMock.Object);
    }

    // [Fact]
    // public async Task CreateRefreshToken_ShouldBeSuccessfull()
    // {
    //     var tokenHash = "tokenHash";
    //     var refreshToken = new RefreshToken
    //     {
    //         UserId = Guid.NewGuid(),
    //         TokenHash = tokenHash,
    //         ExpiresAt = DateTime.UtcNow,
    //         CreatedAt = DateTime.UtcNow,
    //         CreatedByIp = ipAddress,
    //         UserAgent = userAgent,
    //         Jti = Guid.NewGuid()
    //     };
    //     var user = new User { Id = refreshToken.UserId, Email = "Email@mail.com" };
    //     tokenServiceMock.Setup(t => t.CreateRefreshTokenAsync(user)).ReturnsAsync(("rt", tokenHash, DateTime.UtcNow));
    //     refreshTokenRepository.Setup(r => r.AddAsync(refreshToken)).ReturnsAsync(It.IsAny<RefreshToken>());

    //     var result = await refreshTokenService.CreateRefreshTokenAsync(user);
    //     result.Should().NotBeNull();
    //     result.Item1.Should().Be("rt");
    //     result.Item2.TokenHash.Should().Be(tokenHash);

    // }
    [Fact]
    public async Task GetRefreshToken_ShouldBeSuccessfull()
    {
        var tokenHash = "tokenHash";
        var rawToken = "rawToken";
        var refreshToken = new RefreshToken
        {
            UserId = Guid.NewGuid(),
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress,
            UserAgent = userAgent,
            Jti = Guid.NewGuid()
        };
        tokenServiceMock.Setup(t => t.HashToken(rawToken)).Returns(tokenHash);
        refreshTokenRepository.Setup(r => r.GetRefreshTokenByTokenHash(tokenHash)).ReturnsAsync(refreshToken);

        var result = await refreshTokenService.GetRefreshToken(rawToken);
        result.Should().NotBeNull();
        result.TokenHash.Should().Be(tokenHash);

    }
    [Fact]
    public async Task GenerateAndRevokeRefreshToken_ShouldBeSuccessfull()
    {
        var token = "old-refresh-token";
        var rawToken = "rt";
        var tokenHash = "tokenHash";
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "test@test.com" };
        var oldRefreshToken = new RefreshToken
        {
            UserId = userId,
            User = user,
            TokenHash = tokenHash,
            Jti = Guid.NewGuid()
        };
        var newRefreshToken = new RefreshToken
        {
            UserId = userId,
            Id = Guid.NewGuid(),
            Jti = Guid.NewGuid()
        };
        tokenServiceMock.Setup(t => t.HashToken(token)).Returns(tokenHash);
        refreshTokenRepository.Setup(r => r.GetRefreshTokenByTokenHash(tokenHash)).ReturnsAsync(oldRefreshToken);

        refreshTokenRepository.Setup(r => r.AddAsync(It.IsAny<RefreshToken>())).ReturnsAsync(newRefreshToken);
        refreshTokenRepository.Setup(r => r.UpdateAsync(It.IsAny<RefreshToken>())).Returns(Task.CompletedTask);
        tokenServiceMock.Setup(t => t.CreateRefreshTokenAsync(user)).ReturnsAsync((rawToken, newRefreshToken));
        tokenServiceMock.Setup(t => t.CreateAccessToken(It.IsAny<User>(), It.IsAny<Guid>())).Returns("new-access-token");

        var result = await refreshTokenService.GenerateAndRevokeRefreshTokenAsync(token);
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("new-access-token");
        result.RefreshToken.Should().Be(rawToken);
    }
    [Fact]
    public async Task GenerateAndRevokeRefreshToken_ShouldBeNull()
    {
        var token = "old-refresh-token";
        var tokenHash = "tokenHash";
        var userId = Guid.NewGuid();
        var oldRefreshToken = new RefreshToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            Jti = Guid.NewGuid()
        };

        tokenServiceMock.Setup(t => t.HashToken(token)).Returns(tokenHash);
        refreshTokenRepository.Setup(r => r.GetRefreshTokenByTokenHash(tokenHash)).ReturnsAsync(oldRefreshToken);

        var result = await refreshTokenService.GenerateAndRevokeRefreshTokenAsync(token);
        result.Should().BeNull();
    }
    [Fact]
    public async Task RevokePreviousToken_ShouldRevokeAllTokens_WhenAllDeviceIsTrue()
    {
        var userId = Guid.NewGuid();
        var tokens = new List<RefreshToken>
        {
            new RefreshToken { Id = Guid.NewGuid(), UserId = userId },
            new RefreshToken { Id = Guid.NewGuid(), UserId = userId }
        };

        refreshTokenRepository.Setup(r => r.GetRefreshTokensByUserId(userId)).ReturnsAsync(tokens);

        await refreshTokenService.RevokePreviousToken(userId, true);

        foreach (var token in tokens)
        {
            refreshTokenRepository.Verify(r => r.UpdateAsync(It.Is<RefreshToken>(rt => rt.Id == token.Id)), Times.Once);
        }
    }
    [Fact]
    public async Task RevokePreviousToken_ShouldRevokeSingleToken_WhenAllDeviceIsFalse()
    {
        var userId = Guid.NewGuid();
        var token = new RefreshToken { Id = Guid.NewGuid(), UserId = userId };
        refreshTokenRepository
            .Setup(r => r.GetRefreshTokenByUserId(userId, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(token);

        await refreshTokenService.RevokePreviousToken(userId, false);

        refreshTokenRepository.Verify(r => r.UpdateAsync(It.Is<RefreshToken>(rt => rt.Id == token.Id)), Times.Once);
    }
    [Fact]
    public async Task RevokePreviousToken_ShouldDoNothing_WhenAllDeviceIsFalseAndTokenIsNull()
    {
        var userId = Guid.NewGuid();

        refreshTokenRepository.Setup(r => r.GetRefreshTokenByUserId(userId, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((RefreshToken?)null);

        await refreshTokenService.RevokePreviousToken(userId, allDevice: false);

        refreshTokenRepository.Verify(r => r.UpdateAsync(It.IsAny<RefreshToken>()), Times.Never);
    }

}
