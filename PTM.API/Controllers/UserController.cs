using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PTM.Application.Interfaces.Services;
using PTM.Contracts.Requests;
using PTM.Contracts.Response;

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

        
        [HttpGet("{id:guid}", Name ="Get")]
        public async Task<IActionResult> Get(Guid id)
        {
            var res = await userService.GetByIdAsync(id);
            if (res is null) return NotFound("user not found");
            return Ok(ApiResponse<UserResponse>.SuccessResponse(res, "User found successfully", HttpContext.TraceIdentifier));
        }
        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(ApiResponse<IEnumerable<UserResponse>>.SuccessResponse(await userService.GetAllAsync(), "Users found successfully", HttpContext.TraceIdentifier));
        
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UserUpdateRequest request)
        {
            var res = await userService.UpdateAsync(id, request);
            if (res is null) return NotFound("not found");
            return Ok(ApiResponse<UserResponse>.SuccessResponse(res, "User updated successfully", HttpContext.TraceIdentifier));
        }
       
    }
}
