using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PTM.Application.Mappers;
using PTM.Contracts.Requests;
using PTM.Contracts.Requests.TaskItem;
using PTM.Contracts.Response;
using PTM.Contracts.Response.TaskItem;

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
            return CreatedAtAction(nameof(Get), new { id = res.Id },
            ApiResponse<TaskItemResponse>.SuccessResponse(res, "Task created successfully", HttpContext.TraceIdentifier));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var res = await taskItemService.GetByIdAsync(id);
            return Ok(ApiResponse<TaskItemResponse>.SuccessResponse(res, "Task found successfully", HttpContext.TraceIdentifier));
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var res = await taskItemService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<TaskItemResponse>>.SuccessResponse(res, "Tasks found successfully", HttpContext.TraceIdentifier));
        }    
        
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] TaskItemUpdateRequest request)
        {
            var res = await taskItemService.UpdateAsync(id, request);
            return Ok(ApiResponse<TaskItemResponse>.SuccessResponse(res, "Task updated successfully", HttpContext.TraceIdentifier));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await taskItemService.DeleteAsync(id);
            return Ok(ApiResponse<MessageResponse>.SuccessResponse(new MessageResponse { Massage = "Deleted." }, "Task deleted successfully.", HttpContext.TraceIdentifier));
        }
        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest statusRequest)
        {
            var res = await taskItemService.ChangeStatus(id, statusRequest);
            return Ok(ApiResponse<ChangeStatusResponse>.SuccessResponse(res, "Task status changed successfully.", HttpContext.TraceIdentifier));
        }
        [HttpPatch("{id:guid}/priority")]
        public async Task<IActionResult> ChangePriority(Guid id, [FromBody] ChangePriorityRequest priorityRequest)
        {
            var res = await taskItemService.ChangePriority(id, priorityRequest);
            return Ok(ApiResponse<ChangePriorityResponse>.SuccessResponse(res, "Task priority changed successfully.", HttpContext.TraceIdentifier));
        }
       
    }
}
