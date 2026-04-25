using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PfeManagement.Application.DTOs.Tasks;
using PfeManagement.Application.Interfaces;
using PfeManagement.Domain.Interfaces;

namespace PfeManagement.WebApi.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITaskReportService _taskReportService;

        public TasksController(ITaskService taskService, IUnitOfWork unitOfWork, ITaskReportService taskReportService)
        {
            _taskService = taskService;
            _unitOfWork = unitOfWork;
            _taskReportService = taskReportService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
        {
            try
            {
                var createdByUserId = GetCurrentUserId() ?? Guid.Empty;
                var result = await _taskService.CreateTaskAsync(dto, createdByUserId);
                return StatusCode(201, new { success = true, message = "Task created successfully", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(Guid id)
        {
            var task = await _unitOfWork.Tasks.GetByIdAsync(id);
            if (task == null)
            {
                return NotFound(new { success = false, message = "Task not found" });
            }

            return Ok(new
            {
                success = true,
                message = "Task fetched successfully",
                data = new
                {
                    id = task.Id,
                    title = task.Title,
                    description = task.Description,
                    status = task.Status,
                    priority = task.Priority,
                    userStoryId = task.UserStoryId,
                    assignedToId = task.AssignedToId
                }
            });
        }

        [Authorize]
        [HttpPatch("{id}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(Guid id, [FromBody] UpdateTaskDto dto)
        {
            try
            {
                var modifiedByUserId = GetCurrentUserId() ?? Guid.Empty;
                
                var result = await _taskService.UpdateTaskAsync(id, dto, modifiedByUserId);
                return Ok(new { success = true, message = "Task updated successfully", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(Guid id)
        {
            try
            {
                await _taskService.DeleteTaskAsync(id);
                return Ok(new { success = true, message = "Task deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("userstory/{userStoryId}")]
        public async Task<IActionResult> GetTasksForUserStory(Guid userStoryId)
        {
            var tasks = await _taskService.GetTasksAsync(userStoryId);
            return Ok(new { success = true, message = "Tasks fetched successfully", data = tasks });
        }

        [Authorize]
        [HttpGet("history/{task_id}")]
        public async Task<IActionResult> GetTaskHistory(Guid task_id)
        {
            var history = await _unitOfWork.TaskHistories.GetAsync(h => h.TaskId == task_id);
            var data = history.OrderByDescending(h => h.CreatedAt).Select(h => new
            {
                id = h.Id,
                taskId = h.TaskId,
                modifiedById = h.ModifiedById,
                fieldChanged = h.FieldChanged,
                oldValue = h.OldValue,
                newValue = h.NewValue,
                createdAt = h.CreatedAt
            });

            return Ok(data);
        }

        [Authorize]
        [HttpPatch("status/{id}")]
        public async Task<IActionResult> UpdateTaskStatus(Guid id, [FromBody] UpdateTaskStatusRequest request)
        {
            var dto = new UpdateTaskDto { Status = request.Status };
            return await UpdateTask(id, dto);
        }

        [Authorize]
        [HttpPatch("validate/{id}")]
        public async Task<IActionResult> ValidateTaskStatus(Guid id, [FromBody] ValidateTaskStatusRequest request)
        {
            var dto = new UpdateTaskDto { Status = request.Status };
            return await UpdateTask(id, dto);
        }

        [Authorize]
        [HttpGet("report/{projectId}")]
        public async Task<IActionResult> GetProjectTaskReport(Guid projectId)
        {
            var data = await _taskReportService.GetProjectTaskReportAsync(projectId);
            return Ok(new { success = true, message = "Project task report generated", data });
        }

        [Authorize]
        [HttpGet("sprintreport/{sprintId}")]
        public async Task<IActionResult> GetSprintTaskReport(Guid sprintId)
        {
            var data = await _taskReportService.GetSprintTaskReportAsync(sprintId);
            return Ok(new { success = true, message = "Sprint task report generated", data });
        }

        private Guid? GetCurrentUserId()
        {
            var sub = User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type.EndsWith("nameidentifier", StringComparison.OrdinalIgnoreCase))?.Value;
            return Guid.TryParse(sub, out var userId) ? userId : null;
        }

        public class UpdateTaskStatusRequest
        {
            public Domain.Enums.TaskItemStatus Status { get; set; }
        }

        public class ValidateTaskStatusRequest
        {
            public Domain.Enums.TaskItemStatus Status { get; set; }
        }
    }
}
