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

        [HttpPost]
         public async Task<IActionResult> Add([FromBody] UserPlanRequest request)
        {
            var res = await userPlanService.Purchase(request.PlanId);
            return CreatedAtAction(nameof(Get), new { id = res.Id },
            ApiResponse<UserPlanResponse>.SuccessResponse(res, "Plan Bought successfully", HttpContext.TraceIdentifier));
        }
        [HttpGet("id:guid")]
         public async Task<IActionResult> Get(Guid id)
        {
            var res = await userPlanService.GetUserPlanById(id);
            return Ok(ApiResponse<UserPlanResponse>.SuccessResponse(res, "Purchased Plan", HttpContext.TraceIdentifier));
        }
    }
}
