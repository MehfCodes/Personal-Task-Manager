using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PTM.Application.Interfaces.Services;
using PTM.Contracts.Requests;

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
            return CreatedAtAction(nameof(UserController.Get), "User", new { id = res.Id }, res);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            var res = await authService.Login(request);
            if (res is null) return NotFound("user not found");
            return Ok(res);
        }
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var res = await authService.RefreshToken(request.RefreshToken);
            if (res is null) return NotFound("not found");
            return Ok(res);
        }
        [HttpPatch("update-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
        {
            var res = await authService.UpdatePassword(request);
            if (res is null) return NotFound("not found");
            return Ok(res);
        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var res = await authService.Logout();
            if (res is null) return NotFound("not found");
            return Ok(res);
        }
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var res = await authService.ForgotPassword(request);
            if (res is null) return NotFound("not found");
            return Ok(res);
        }
    }
}
