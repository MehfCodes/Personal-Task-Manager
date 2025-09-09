using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PTM.Application.Services;
using PTM.Contracts.Requests;
using PTM.Contracts.Response;

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
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] PlanRequest request)
        {
            var result = await planService.AddAsync(request);
            return CreatedAtAction(nameof(Get),new {id = result.Id},
            ApiResponse<PlanResponse>.SuccessResponse(result, "Plan created successfully", HttpContext.TraceIdentifier));
        }
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var res = await planService.GetByIdAsync(id);
            if (res is null) return NotFound("not found");
            return Ok(ApiResponse<PlanResponse>.SuccessResponse(res, "Plan found successfully", HttpContext.TraceIdentifier));
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var res = await planService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<PlanResponse>>.SuccessResponse(res, "Plans found successfully", HttpContext.TraceIdentifier));
        }
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PlanUpdateRequest request)
        {
            var res = await planService.UpdateAsync(id, request);
            if (res is null) return NotFound("not found");
            return Ok(ApiResponse<PlanResponse>.SuccessResponse(res, "Plan updated successfully", HttpContext.TraceIdentifier));
        }

        [HttpPatch("{id:guid}/deactive")]
        public async Task<IActionResult> DeActive(Guid id)
        {
            var res = await planService.DeActiveAsync(id);
            if (!res) return BadRequest("Deactivation failed!");
            var msg = new MessageResponse { Massage = "Deactived" };
            return Ok(ApiResponse<MessageResponse>.SuccessResponse(msg, "Plan deactived successfully", HttpContext.TraceIdentifier));
        }
        [HttpPatch("{id:guid}/active")]
        public async Task<IActionResult> Activate(Guid id)
        {
            var res = await planService.ActivateAsync(id);
            if (!res) return BadRequest("Activation Failed!");
            var msg = new MessageResponse { Massage = "Actived" };
            return Ok(ApiResponse<MessageResponse>.SuccessResponse(msg, "Plan actived successfully", HttpContext.TraceIdentifier));
        }
    }
}
