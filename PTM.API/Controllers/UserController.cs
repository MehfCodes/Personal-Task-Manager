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
        public async Task<IActionResult> Add([FromBody] UserRegisterRequest request)
        {
            var res = await userService.AddAsync(request);
            return CreatedAtAction(nameof(Get), new { id = res.Id }, res);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var res = await userService.GetByIdAsync(id);
            if (res is null) return NotFound("user not found");
            return Ok(res);
        }
        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await userService.GetAllAsync());
        
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UserUpdateRequest request)
        {
            var res = await userService.UpdateAsync(id, request);
            if (res is null) return NotFound("not found");
            return Ok(res);
        }
       
    }
}
