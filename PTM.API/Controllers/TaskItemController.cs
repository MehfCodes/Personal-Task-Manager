using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PTM.Application.Mappers;
using PTM.Contracts.Requests;

namespace PTM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskItemController : ControllerBase
    {
        private readonly ITaskItemService taskItemService;

        public TaskItemController(ITaskItemService taskItemService)
        {
            this.taskItemService = taskItemService;
        }
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] TaskItemRequest request)
        {
            var res = await taskItemService.AddAsync(request);
            return CreatedAtAction(nameof(Get), new { id = res.Id }, res);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var res = await taskItemService.GetByIdAsync(id);
            if (res is null) return NotFound("user not found");
            return Ok(res);
        }
        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await taskItemService.GetAllAsync());
        
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] TaskItemUpdateRequest request)
        {
            var res = await taskItemService.UpdateAsync(id, request);
            if (res is null) return NotFound("not found");
            return Ok(res);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var res = await taskItemService.DeleteAsync(id);
            if (res is false) return NotFound("not found");
            return Ok("Deleted Successfully");
        }
       
    }
}
