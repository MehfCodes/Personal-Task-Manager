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
        public IActionResult UpdatePassword([FromBody] UpdatePasswordRequest request)
        {
            var res = authService.UpdatePassword(request);
            return Ok(res);
        }
    }
}
