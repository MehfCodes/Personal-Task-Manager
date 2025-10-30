using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PTM.Application.Services;
using PTM.Contracts.Requests;
using PTM.Contracts.Response;
using Swashbuckle.AspNetCore.Annotations;

namespace PTM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlanController : ControllerBase
    {
        private readonly IPlanService planService;

        public PlanController(IPlanService planService)
        {
            this.planService = planService;
        }

        /// <summary>
        /// Create a new plan.
        /// </summary>
        /// <param name="request">Plan details.</param>
        /// <returns>The created plan.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [SwaggerOperation(Summary = "Add new plan", Description = "Creates a new plan and returns it.")]
        [ProducesResponseType(typeof(ApiResponse<PlanResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Add([FromBody] PlanRequest request)
        {
            var result = await planService.AddAsync(request);
            return CreatedAtAction(nameof(Get), new { id = result.Id },
                ApiResponse<PlanResponse>.SuccessResponse(result, "Plan created successfully", HttpContext.TraceIdentifier));
        }

        /// <summary>
        /// Get a plan by its ID.
        /// </summary>
        /// <param name="id">Plan unique identifier.</param>
        /// <returns>The plan if found.</returns>
        [Authorize(Roles = "Admin,User")]
        [HttpGet("{id:guid}")]
        [SwaggerOperation(Summary = "Get plan by ID", Description = "Returns the plan with the specified ID.")]
        [ProducesResponseType(typeof(ApiResponse<PlanResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(Guid id)
        {
            var res = await planService.GetByIdAsync(id);
            return Ok(ApiResponse<PlanResponse>.SuccessResponse(res, "Plan found successfully", HttpContext.TraceIdentifier));
        }

        /// <summary>
        /// Get all plans.
        /// </summary>
        /// <returns>List of all plans.</returns>
        [Authorize(Roles = "Admin,User")]
        [HttpGet]
        [SwaggerOperation(Summary = "Get all plans", Description = "Returns all available plans.")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PlanResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var res = await planService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<PlanResponse>>.SuccessResponse(res, "Plans found successfully", HttpContext.TraceIdentifier));
        }

        /// <summary>
        /// Update an existing plan.
        /// </summary>
        /// <param name="id">Plan unique identifier.</param>
        /// <param name="request">Updated plan details.</param>
        /// <returns>The updated plan.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}")]
        [SwaggerOperation(Summary = "Update plan", Description = "Updates an existing plan.")]
        [ProducesResponseType(typeof(ApiResponse<PlanResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] PlanUpdateRequest request)
        {
            var res = await planService.UpdateAsync(id, request);
            return Ok(ApiResponse<PlanResponse>.SuccessResponse(res, "Plan updated successfully", HttpContext.TraceIdentifier));
        }

        /// <summary>
        /// Deactivate a plan.
        /// </summary>
        /// <param name="id">Plan unique identifier.</param>
        /// <returns>Status message.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id:guid}/deactive")]
        [SwaggerOperation(Summary = "Deactivate plan", Description = "Deactivates the specified plan.")]
        [ProducesResponseType(typeof(ApiResponse<MessageResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeActive(Guid id)
        {
            await planService.DeActiveAsync(id);
            var msg = new MessageResponse { Massage = "Deactived" };
            return Ok(ApiResponse<MessageResponse>.SuccessResponse(msg, "Plan deactived successfully", HttpContext.TraceIdentifier));
        }

        /// <summary>
        /// Activate a plan.
        /// </summary>
        /// <param name="id">Plan unique identifier.</param>
        /// <returns>Status message.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id:guid}/active")]
        [SwaggerOperation(Summary = "Activate plan", Description = "Activates the specified plan.")]
        [ProducesResponseType(typeof(ApiResponse<MessageResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Activate(Guid id)
        {
            await planService.ActivateAsync(id);
            var msg = new MessageResponse { Massage = "Actived" };
            return Ok(ApiResponse<MessageResponse>.SuccessResponse(msg, "Plan actived successfully", HttpContext.TraceIdentifier));
        }
    }
}
