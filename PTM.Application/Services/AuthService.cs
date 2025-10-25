using Microsoft.Extensions.Logging;
using PTM.Application.Exceptions;
using PTM.Application.Interfaces;
using PTM.Application.Interfaces.Authentication;
using PTM.Application.Interfaces.Services;
using PTM.Application.Mappers;
using PTM.Contracts.Requests;
using PTM.Contracts.Response;
using PTM.Contracts.Response.User;

namespace PTM.Application.Services;

public class AuthService : BaseService, IAuthService
{
    private readonly IUserRepository repository;
    private readonly ITokenService tokenService;
    private readonly IRefreshTokenService refreshTokenService;
    private readonly IRequestContext requestContext;
    private readonly IPasswordHasher passwordHasher;
    private readonly ILogger<AuthService> logger;
    public AuthService(IUserRepository repository,
    ITokenService tokenService,
    IRefreshTokenService refreshTokenService,
    IRequestContext requestContext,
    IServiceProvider serviceProvider,
    IPasswordHasher passwordHasher,
    ILogger<AuthService> logger) : base(serviceProvider)
    {
        this.repository = repository;
        this.tokenService = tokenService;
        this.refreshTokenService = refreshTokenService;
        this.requestContext = requestContext;
        this.passwordHasher = passwordHasher;
        this.logger = logger;
    }

    public async Task<UserResponse> Register(UserRegisterRequest request)
    {
        await ValidateAsync(request);
        var newUser = request.MapToUser();
        var record = await repository.AddAsync(newUser);
        var res = record.MapToUserResponse();
        var tokens = await tokenService.GenerateTokenPair(newUser);
        res.AccessToken = tokens.AccessToken;
        res.RefreshToken = tokens.RefreshToken;
        logger.LogInformation("User {UserId} Registered at {Time}", res.Id, DateTime.UtcNow);
        return res;
    }

    public async Task<UserResponse> Login(UserLoginRequest request)
    {
        await ValidateAsync(request);
        var user = await repository.GetUserByEmail(request.Email!);
        if (user is null || !passwordHasher.VerifyPassword(request.Password!, user.Password)) throw new UnauthorizedException("Invalid username or password.");
        var res = user.MapToUserResponse();
        await refreshTokenService.RevokePreviousToken(user.Id);
        var tokens = await tokenService.GenerateTokenPair(user);
        res.AccessToken = tokens.AccessToken;
        res.RefreshToken = tokens.RefreshToken;
        logger.LogInformation("User {UserId} logged in at {Time}", res.Id, DateTime.UtcNow);
        return res;
    }

    public async Task<RefreshTokenResponse> RefreshToken(RefreshTokenRequest request)
    {
        await ValidateAsync(request);
        var userIdReq = requestContext.GetUserId();
        var rt = await refreshTokenService.GenerateAndRevokeRefreshTokenAsync(request.RefreshToken);
        if (rt is null) throw new NotFoundException("Your session has expired.");
        logger.LogInformation("Token Refreshed for User {UserId} at {Time}", userIdReq!.Value, DateTime.UtcNow);
        return new RefreshTokenResponse { AccessToken = rt.AccessToken, RefreshToken = rt.RefreshToken };
    }

    public async Task<LogoutResponse> Logout()
    {
        var userIdReq = requestContext.GetUserId();
        if (!userIdReq.HasValue) throw new UnauthorizedException();
        await refreshTokenService.RevokePreviousToken(userIdReq.Value);
        logger.LogInformation("User {UserId} logged out at {Time}", userIdReq.Value, DateTime.UtcNow);
        return new LogoutResponse { Massage = "you logout." };
    }
}
