using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PTM.Application.Interfaces.Services;
using PTM.Contracts.Requests;

namespace PTM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService userService;

        public UserController(IUserService userService)
        {
            this.userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] UserRequest request)
        {
            var res = await userService.AddAsync(request);
            return Ok(res);
        }
    }
}
