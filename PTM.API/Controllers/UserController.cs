using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PTM.Application.Interfaces.Services;
using PTM.Contracts.Requests;
using PTM.Contracts.Response;

namespace PTM.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService userService;

        public UserController(IUserService userService)
        {
            this.userService = userService;
        }

        /// <summary>
        /// Get a user by Id.
        /// </summary>
        /// <param name="id">User Id.</param>
        /// <returns>User details.</returns>
        /// <response code="200">Returns the user details.</response>
        /// <response code="404">If the user is not found.</response>
        [HttpGet("{id:guid}", Name ="Get")]
        [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(Guid id)
        {
            var res = await userService.GetByIdAsync(id);
            return Ok(ApiResponse<UserResponse>.SuccessResponse(res, "User found successfully", HttpContext.TraceIdentifier));
        }

        /// <summary>
        /// Get all users.
        /// </summary>
        /// <returns>List of users.</returns>
        /// <response code="200">Returns a list of users.</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll() =>
            Ok(ApiResponse<IEnumerable<UserResponse>>.SuccessResponse(
                await userService.GetAllAsync(), 
                "Users found successfully", 
                HttpContext.TraceIdentifier));

        /// <summary>
        /// Update an existing user.
        /// </summary>
        /// <param name="id">User Id.</param>
        /// <param name="request">Updated user details.</param>
        /// <returns>Updated user.</returns>
        /// <response code="200">Returns the updated user.</response>
        /// <response code="404">If the user is not found.</response>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UserUpdateRequest request)
        {
            var res = await userService.UpdateAsync(id, request);
            return Ok(ApiResponse<UserResponse>.SuccessResponse(res, "User updated successfully", HttpContext.TraceIdentifier));
        }

         /// <summary>
        /// Promoted a user to admin.
        /// </summary>
        /// <param name="id">User Id.</param>
        /// <returns>Status Message.</returns>
        /// <response code="200">Returns the Successfull Message.</response>
        /// <response code="404">If the user is not found.</response>
        [HttpPatch("PromoteToAdmin/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<MessageResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PromoteToAdmin(Guid id)
        {
            var res = await userService.PromoteToAdminAsync(id);
            return Ok(ApiResponse<MessageResponse>.SuccessResponse(res, "User promoted successfully", HttpContext.TraceIdentifier));
        }
    }
}
