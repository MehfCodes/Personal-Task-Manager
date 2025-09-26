using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PTM.Application.Interfaces.Services;
using PTM.Contracts.Requests.UserPlan;
using PTM.Contracts.Response;
using PTM.Contracts.Response.UserPlan;

namespace PTM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserPlanController : ControllerBase
    {
        private readonly IUserPlanService userPlanService;

        public UserPlanController(IUserPlanService userPlanService)
        {
            this.userPlanService = userPlanService;
        }

        /// <summary>
        /// Purchase a plan for the current user.
        /// </summary>
        /// <param name="request">The plan purchase request.</param>
        /// <returns>The purchased plan details.</returns>
        /// <response code="201">Returns the purchased plan details.</response>
        /// <response code="400">If the request is invalid or plan cannot be purchased.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<UserPlanResponseDetail>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Add([FromBody] UserPlanRequest request)
        {
            var res = await userPlanService.Purchase(request.PlanId);
            return CreatedAtAction(nameof(Get), new { id = res.Id },
            ApiResponse<UserPlanResponseDetail>.SuccessResponse(res, "Plan Bought successfully", HttpContext.TraceIdentifier));
        }
        
        /// <summary>
        /// Get a purchased plan by its Id.
        /// </summary>
        /// <param name="id">The user plan Id.</param>
        /// <returns>The purchased plan details.</returns>
        /// <response code="200">Returns the purchased plan details.</response>
        /// <response code="404">If the plan is not found.</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<UserPlanResponseDetail>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(Guid id)
        {
            var res = await userPlanService.GetUserPlanById(id);
            return Ok(ApiResponse<UserPlanResponseDetail>.SuccessResponse(res, "Purchased Plan", HttpContext.TraceIdentifier));
        }
         /// <summary>
        /// Deactive a purchased plan by its Id.
        /// </summary>
        /// <param name="id">The user plan Id.</param>
        /// <returns>Status message.</returns>
        /// <response code="200">Returns a message.</response>
        /// <response code="404">If the plan is not found.</response>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<MessageResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Deactive(Guid id)
        {
            var res = await userPlanService.DeactivateAsync(id);
            return Ok(ApiResponse<MessageResponse>.SuccessResponse(res, "Plan Deactivated Successfully", HttpContext.TraceIdentifier));
        }
    }
}
