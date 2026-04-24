using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PfeManagement.WebApi.Data;
using PfeManagement.WebApi.Models;
using PfeManagement.WebApi.Interfaces;

namespace PfeManagement.WebApi.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly INotificationService _notificationService;

        public TasksController(AppDbContext db, INotificationService notificationService)
        {
            _db = db;
            _notificationService = notificationService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
        {
            try
            {
                var task = new TaskItem
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    Status = dto.Status,
                    Priority = dto.Priority,
                    UserStoryId = dto.UserStoryId,
                    AssignedToId = dto.AssignedToId
                };

                _db.Tasks.Add(task);
                await _db.SaveChangesAsync();

                return StatusCode(201, new { success = true, message = "Task created successfully", data = MapToDto(task) });
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
            var task = await _db.Tasks.FindAsync(id);
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

                var task = await _db.Tasks.FindAsync(id);
                if (task == null) throw new Exception("Task not found");

                var oldStatus = task.Status;

                if (dto.Title != null) task.Title = dto.Title;
                if (dto.Description != null) task.Description = dto.Description;
                if (dto.Priority.HasValue) task.Priority = dto.Priority.Value;
                if (dto.AssignedToId.HasValue) task.AssignedToId = dto.AssignedToId.Value;

                if (dto.Status.HasValue && dto.Status != task.Status)
                {
                    task.Status = dto.Status.Value;

                    // Inline task history recording (was Observer pattern before)
                    var history = new TaskHistory
                    {
                        TaskId = task.Id,
                        ModifiedById = modifiedByUserId,
                        FieldChanged = "status",
                        OldValue = oldStatus.ToString(),
                        NewValue = task.Status.ToString()
                    };
                    _db.TaskHistories.Add(history);

                    // Inline supervisor notification (was Observer pattern before)
                    if (task.Status == TaskItemStatus.Done)
                    {
                        try
                        {
                            await _notificationService.SendAsync(
                                "supervisor@pfemanagement.com",
                                "Task Completed",
                                $"Task {task.Id} was marked as Done.");
                        }
                        catch { }
                    }
                }

                await _db.SaveChangesAsync();

                return Ok(new { success = true, message = "Task updated successfully", data = MapToDto(task) });
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
                var task = await _db.Tasks.FindAsync(id);
                if (task == null) throw new Exception("Task not found");

                _db.Tasks.Remove(task);
                await _db.SaveChangesAsync();

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
            var tasks = await _db.Tasks.Where(t => t.UserStoryId == userStoryId).ToListAsync();
            return Ok(new { success = true, message = "Tasks fetched successfully", data = tasks.Select(MapToDto) });
        }

        [Authorize]
        [HttpGet("history/{task_id}")]
        public async Task<IActionResult> GetTaskHistory(Guid task_id)
        {
            var history = await _db.TaskHistories.Where(h => h.TaskId == task_id).OrderByDescending(h => h.CreatedAt).ToListAsync();
            var data = history.Select(h => new
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
            var sprints = await _db.Sprints.Where(s => s.ProjectId == projectId).ToListAsync();
            var sprintIds = sprints.Select(s => s.Id).ToHashSet();
            var userStories = await _db.UserStories.Where(us => sprintIds.Contains(us.SprintId)).ToListAsync();
            var storyIds = userStories.Select(us => us.Id).ToHashSet();
            var tasks = await _db.Tasks.Where(t => storyIds.Contains(t.UserStoryId)).ToListAsync();

            var data = new
            {
                projectId,
                totalTasks = tasks.Count,
                byStatus = tasks.GroupBy(t => t.Status).ToDictionary(g => g.Key.ToString(), g => g.Count())
            };

            return Ok(new { success = true, message = "Project task report generated", data });
        }

        [Authorize]
        [HttpGet("sprintreport/{sprintId}")]
        public async Task<IActionResult> GetSprintTaskReport(Guid sprintId)
        {
            var userStories = await _db.UserStories.Where(us => us.SprintId == sprintId).ToListAsync();
            var storyIds = userStories.Select(us => us.Id).ToHashSet();
            var tasks = await _db.Tasks.Where(t => storyIds.Contains(t.UserStoryId)).ToListAsync();

            var data = new
            {
                sprintId,
                totalTasks = tasks.Count,
                byStatus = tasks.GroupBy(t => t.Status).ToDictionary(g => g.Key.ToString(), g => g.Count())
            };

            return Ok(new { success = true, message = "Sprint task report generated", data });
        }

        private Guid? GetCurrentUserId()
        {
            var sub = User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type.EndsWith("nameidentifier", StringComparison.OrdinalIgnoreCase))?.Value;
            return Guid.TryParse(sub, out var userId) ? userId : null;
        }

        private static TaskResponseDto MapToDto(TaskItem task)
        {
            return new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                UserStoryId = task.UserStoryId,
                AssignedToId = task.AssignedToId
            };
        }

        public class UpdateTaskStatusRequest
        {
            public TaskItemStatus Status { get; set; }
        }

        public class ValidateTaskStatusRequest
        {
            public TaskItemStatus Status { get; set; }
        }
    }
}
