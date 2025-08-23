using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PTM.Application.Services;
using PTM.Contracts.Requests;

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
            return Ok(result);
        }
    //     public async Task<IActionResult> Get(Guid id){}
    //     public async Task<IActionResult> GetAll(){}
    //     public async Task<IActionResult> Update(Guid id, [FromBody] PlanRequest request){}
    //     public async Task<IActionResult> Delete(Guid id){}
    }
}
