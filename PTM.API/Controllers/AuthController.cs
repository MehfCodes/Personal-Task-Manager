using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PTM.Application.Interfaces.Services;
using PTM.Contracts.Requests;
using PTM.Contracts.Requests.User;
using PTM.Contracts.Response;
using PTM.Contracts.Response.User;
using Swashbuckle.AspNetCore.Annotations;

namespace PTM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService authService;
        private readonly IUserPasswordService userPasswordService;

        public AuthController(IAuthService authService, IUserPasswordService userPasswordService)
        {
            this.authService = authService;
            this.userPasswordService = userPasswordService;
        }

        /// <summary>
        /// Register a new user account.
        /// </summary>
        /// <param name="request">User registration details.</param>
        /// <returns>User information after successful registration.</returns>
        [AllowAnonymous]
        [HttpPost("register")]
        [SwaggerOperation(Summary = "Register new user", Description = "Creates a new user account.")]
        [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequest request)
        {
            var res = await authService.Register(request);
            return CreatedAtAction(nameof(UserController.Get), "User", new { id = res.Id },
             ApiResponse<UserResponse>.SuccessResponse(res, "User register successfully", HttpContext.TraceIdentifier, 201));
        }

        /// <summary>
        /// Authenticate a user and return JWT token.
        /// </summary>
        /// <param name="request">Login credentials.</param>
        /// <returns>User information with access token.</returns>
        [AllowAnonymous]
        [HttpPost("login")]
        [SwaggerOperation(Summary = "User login", Description = "Logs user in and returns JWT token.")]
        [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            var res = await authService.Login(request);
            return Ok(ApiResponse<UserResponse>.SuccessResponse(res, "User login successfully", HttpContext.TraceIdentifier));
        }

        /// <summary>
        /// Refresh JWT token using a valid refresh token.
        /// </summary>
        /// <param name="request">Refresh token request data.</param>
        /// <returns>New access and refresh token.</returns>
        [Authorize(Roles = "Admin,User")]
        [HttpPost("refresh")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Refresh JWT token", Description = "Generates new access token using refresh token.")]
        [ProducesResponseType(typeof(ApiResponse<RefreshTokenResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var res = await authService.RefreshToken(request);
            return Ok(ApiResponse<RefreshTokenResponse>.SuccessResponse(res, "Refresh token generated.", HttpContext.TraceIdentifier));
        }

        /// <summary>
        /// Update user password.
        /// </summary>
        /// <param name="request">Password update details.</param>
        /// <returns>Status of password update.</returns>
        [Authorize(Roles = "Admin,User")]
        [HttpPatch("update-password")]
        [SwaggerOperation(Summary = "Update user password", Description = "Changes the user's password.")]
        [ProducesResponseType(typeof(ApiResponse<UpdatePasswordResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
        {
            var res = await userPasswordService.UpdatePassword(request);
            return Ok(ApiResponse<UpdatePasswordResponse>.SuccessResponse(res, "Password update successfully", HttpContext.TraceIdentifier));
        }

        /// <summary>
        /// Logout the current user and revoke refresh token.
        /// </summary>
        /// <returns>Logout status.</returns>
        [Authorize(Roles = "Admin,User")]
        [HttpPost("logout")]
        [SwaggerOperation(Summary = "Logout user", Description = "Logs out the current user and revokes refresh token.")]
        [ProducesResponseType(typeof(ApiResponse<LogoutResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout()
        {
            var res = await authService.Logout();
            return Ok(ApiResponse<LogoutResponse>.SuccessResponse(res, "User logout successfully", HttpContext.TraceIdentifier));
        }

        /// <summary>
        /// Request password reset by email.
        /// </summary>
        /// <param name="request">Email address for reset.</param>
        /// <returns>Confirmation message to check email inbox.</returns>
        [Authorize(Roles = "Admin,User")]
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Forgot password", Description = "Sends a reset link to user's email.")]
        [ProducesResponseType(typeof(ApiResponse<ForgotPasswordResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var res = await userPasswordService.ForgotPassword(request);
            return Ok(ApiResponse<ForgotPasswordResponse>.SuccessResponse(res, "Check your email inbox", HttpContext.TraceIdentifier));
        }

        /// <summary>
        /// Reset password using reset token.
        /// </summary>
        /// <param name="request">Reset password request data.</param>
        /// <returns>Status of reset action.</returns>
        [Authorize(Roles = "Admin,User")]
        [HttpPost("reset-password")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Reset password", Description = "Resets password using reset token.")]
        [ProducesResponseType(typeof(ApiResponse<ResetPasswordResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var res = await userPasswordService.ResetPassword(request);
            return Ok(ApiResponse<ResetPasswordResponse>.SuccessResponse(res, "Password reset successfully", HttpContext.TraceIdentifier));
        }
    }
}
