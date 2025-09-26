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

        /// <summary>
        /// Add a new task item.
        /// </summary>
        /// <param name="request">Task item details.</param>
        /// <returns>Created task item.</returns>
        /// <response code="201">Returns the newly created task item.</response>
        /// <response code="400">If the request is invalid.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<TaskItemResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Add([FromBody] TaskItemRequest request)
        {
            var res = await taskItemService.AddAsync(request);
            return CreatedAtAction(nameof(Get), new { id = res.Id },
                ApiResponse<TaskItemResponse>.SuccessResponse(res, "Task created successfully", HttpContext.TraceIdentifier));
        }

        /// <summary>
        /// Get a task item by Id.
        /// </summary>
        /// <param name="id">Task item Id.</param>
        /// <returns>Task item details.</returns>
        /// <response code="200">Returns the task item.</response>
        /// <response code="404">If the task item is not found.</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<TaskItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(Guid id)
        {
            var res = await taskItemService.GetByIdAsync(id);
            return Ok(ApiResponse<TaskItemResponse>.SuccessResponse(res, "Task found successfully", HttpContext.TraceIdentifier));
        }

        /// <summary>
        /// Get all task items.
        /// </summary>
        /// <returns>List of task items.</returns>
        /// <response code="200">Returns the list of task items.</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<TaskItemResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var res = await taskItemService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<TaskItemResponse>>.SuccessResponse(res, "Tasks found successfully", HttpContext.TraceIdentifier));
        }

        /// <summary>
        /// Update an existing task item.
        /// </summary>
        /// <param name="id">Task item Id.</param>
        /// <param name="request">Updated task item details.</param>
        /// <returns>Updated task item.</returns>
        /// <response code="200">Returns the updated task item.</response>
        /// <response code="404">If the task item is not found.</response>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<TaskItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] TaskItemUpdateRequest request)
        {
            var res = await taskItemService.UpdateAsync(id, request);
            return Ok(ApiResponse<TaskItemResponse>.SuccessResponse(res, "Task updated successfully", HttpContext.TraceIdentifier));
        }

        /// <summary>
        /// Delete a task item.
        /// </summary>
        /// <param name="id">Task item Id.</param>
        /// <returns>Confirmation message.</returns>
        /// <response code="200">If the task was deleted successfully.</response>
        /// <response code="404">If the task item is not found.</response>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<MessageResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await taskItemService.DeleteAsync(id);
            return Ok(ApiResponse<MessageResponse>.SuccessResponse(
                new MessageResponse { Massage = "Deleted." }, 
                "Task deleted successfully.", 
                HttpContext.TraceIdentifier));
        }

        /// <summary>
        /// Change status of a task item.
        /// </summary>
        /// <param name="id">Task item Id.</param>
        /// <param name="statusRequest">New status details.</param>
        /// <returns>Updated status information.</returns>
        /// <response code="200">If the status was changed successfully.</response>
        /// <response code="404">If the task item is not found.</response>
        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<ChangeStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest statusRequest)
        {
            var res = await taskItemService.ChangeStatus(id, statusRequest);
            return Ok(ApiResponse<ChangeStatusResponse>.SuccessResponse(res, "Task status changed successfully.", HttpContext.TraceIdentifier));
        }

        /// <summary>
        /// Change priority of a task item.
        /// </summary>
        /// <param name="id">Task item Id.</param>
        /// <param name="priorityRequest">New priority details.</param>
        /// <returns>Updated priority information.</returns>
        /// <response code="200">If the priority was changed successfully.</response>
        /// <response code="404">If the task item is not found.</response>
        [HttpPatch("{id:guid}/priority")]
        [ProducesResponseType(typeof(ApiResponse<ChangePriorityResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangePriority(Guid id, [FromBody] ChangePriorityRequest priorityRequest)
        {
            var res = await taskItemService.ChangePriority(id, priorityRequest);
            return Ok(ApiResponse<ChangePriorityResponse>.SuccessResponse(res, "Task priority changed successfully.", HttpContext.TraceIdentifier));
        }
    }
}
