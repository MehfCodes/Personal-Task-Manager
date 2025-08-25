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
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await planService.GetAllAsync());
        }
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PlanUpdateRequest request)
        {
            var res = await planService.UpdateAsync(id, request);
            if (res is null) return NotFound("not found");
            return Ok(res);
        }

        [HttpPatch("{id:guid}/deactive")]
        public async Task<IActionResult> DeActive(Guid id)
        {
            var res = await planService.DeActiveAsync(id);
            if (!res) return BadRequest("Deactivation failed!");
            return Ok("Deactived");
        }
        [HttpPatch("{id:guid}/active")]
        public async Task<IActionResult> Activate(Guid id)
        {
            var res = await planService.ActivateAsync(id);
            if (!res) return BadRequest("Activation Failed!");
            return Ok("Activated");
        }
    }
}
