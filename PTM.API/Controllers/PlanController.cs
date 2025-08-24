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
            return CreatedAtAction(nameof(Get),new {id = result.Id}, result);
        }
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var res = await planService.GetByIdAsync(id);
            if (res is null) return NotFound("not found");
            return Ok(res);
        }
    //     public async Task<IActionResult> GetAll(){}
    //     public async Task<IActionResult> Update(Guid id, [FromBody] PlanRequest request){}
    //     public async Task<IActionResult> Delete(Guid id){}
    }
}
