using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PTM.Application.Interfaces.Services;
using PTM.Contracts.Requests;
using PTM.Contracts.Requests.User;
using PTM.Contracts.Response;
using PTM.Contracts.Response.User;

namespace PTM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService authService;

        public AuthController(IAuthService authService)
        {
            this.authService = authService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequest request)
        {
            var res = await authService.Register(request);
            return CreatedAtAction(nameof(UserController.Get), "User", new { id = res.Id },
             ApiResponse<UserResponse>.SuccessResponse(res, "User register successfully", HttpContext.TraceIdentifier, 201));
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            var res = await authService.Login(request);
            return Ok(ApiResponse<UserResponse>.SuccessResponse(res, "User login successfully", HttpContext.TraceIdentifier));
        }
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var res = await authService.RefreshToken(request);
            return Ok(ApiResponse<RefreshTokenResponse>.SuccessResponse(res, "Refresh token generated.", HttpContext.TraceIdentifier));
        }
        [HttpPatch("update-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
        {
            var res = await authService.UpdatePassword(request);
            return Ok(ApiResponse<UpdatePasswordResponse>.SuccessResponse(res, "Password update successfully", HttpContext.TraceIdentifier));
        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var res = await authService.Logout();
            return Ok(ApiResponse<LogoutResponse>.SuccessResponse(res, "User logout successfully", HttpContext.TraceIdentifier));
        }
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var res = await authService.ForgotPassword(request);
            return Ok(ApiResponse<ForgotPasswordResponse>.SuccessResponse(res, "Check your email inbox", HttpContext.TraceIdentifier));
        }
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var res = await authService.ResetPassword(request);
            return Ok(ApiResponse<ResetPasswordResponse>.SuccessResponse(res, "Password reset successfully", HttpContext.TraceIdentifier));
        }
    }
}
