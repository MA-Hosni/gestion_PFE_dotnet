using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PfeManagement.Domain.Interfaces;

namespace PfeManagement.WebApi.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [Authorize]
        [HttpGet("projects")]
        public async Task<IActionResult> GetAllProjects()
        {
            var projects = await _unitOfWork.Projects.GetAllAsync();
            return Ok(new { success = true, message = "Projects fetched successfully", data = projects });
        }

        [Authorize]
        [HttpGet("student/timeline")]
        public async Task<IActionResult> GetStudentTimeline()
        {
            var projectId = await GetCurrentProjectIdAsync();
            if (!projectId.HasValue)
            {
                return Ok(new { success = true, message = "Timeline fetched successfully", data = Array.Empty<object>() });
            }

            var sprints = await _unitOfWork.Sprints.GetAsync(s => s.ProjectId == projectId.Value);
            return Ok(new { success = true, message = "Timeline fetched successfully", data = sprints.OrderBy(s => s.StartDate) });
        }

        [Authorize]
        [HttpGet("student/tasks/standby")]
        public async Task<IActionResult> GetStudentStandbyTasks()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "User not authenticated" });
            }

            var tasks = await _unitOfWork.Tasks.GetAsync(t => t.AssignedToId == userId.Value && t.Status == Domain.Enums.TaskItemStatus.ToDo);
            return Ok(new { success = true, message = "Standby tasks fetched successfully", data = tasks });
        }

        [Authorize]
        [HttpGet("{projectId}")]
        public async Task<IActionResult> GetProgress(Guid projectId)
        {
            var sprints = await _unitOfWork.Sprints.GetAsync(s => s.ProjectId == projectId);
            var sprintIds = sprints.Select(s => s.Id).ToHashSet();
            var stories = await _unitOfWork.UserStories.GetAsync(us => sprintIds.Contains(us.SprintId));
            var storyIds = stories.Select(us => us.Id).ToHashSet();
            var tasks = await _unitOfWork.Tasks.GetAsync(t => storyIds.Contains(t.UserStoryId));

            var data = new
            {
                projectId,
                totalTasks = tasks.Count,
                completedTasks = tasks.Count(t => t.Status == Domain.Enums.TaskItemStatus.Done),
                completionRate = tasks.Count == 0 ? 0 : Math.Round(tasks.Count(t => t.Status == Domain.Enums.TaskItemStatus.Done) * 100.0 / tasks.Count, 2)
            };

            return Ok(new { success = true, message = "Progress fetched successfully", data });
        }

        [Authorize]
        [HttpGet("supervisor/timeline")]
        public async Task<IActionResult> GetSupervisorTimeline()
        {
            var meetings = await _unitOfWork.Meetings.GetAllAsync();
            return Ok(new { success = true, message = "Supervisor timeline fetched successfully", data = meetings.OrderByDescending(m => m.ScheduledDate) });
        }

        [Authorize]
        [HttpGet("supervisor/validations/{projectId}")]
        public async Task<IActionResult> GetSupervisorPendingValidations(Guid projectId)
        {
            var sprints = await _unitOfWork.Sprints.GetAsync(s => s.ProjectId == projectId);
            var sprintIds = sprints.Select(s => s.Id).ToHashSet();
            var stories = await _unitOfWork.UserStories.GetAsync(us => sprintIds.Contains(us.SprintId));
            var storyIds = stories.Select(us => us.Id).ToHashSet();
            var tasks = await _unitOfWork.Tasks.GetAsync(t => storyIds.Contains(t.UserStoryId));
            var taskIds = tasks.Select(t => t.Id).ToHashSet();
            var pending = await _unitOfWork.Validations.GetAsync(v => taskIds.Contains(v.TaskId) && v.Status == Domain.Enums.ValidationStatus.Pending);
            return Ok(new { success = true, message = "Pending validations fetched successfully", data = pending });
        }

        [Authorize]
        [HttpGet("supervisor/meetings/{projectId}")]
        public async Task<IActionResult> GetSupervisorLatestMeetings(Guid projectId)
        {
            var meetings = await _unitOfWork.Meetings.GetAsync(m => m.ProjectId == projectId);
            return Ok(new { success = true, message = "Latest meetings fetched successfully", data = meetings.OrderByDescending(m => m.ScheduledDate).Take(10) });
        }

        private Guid? GetCurrentUserId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(sub, out var userId) ? userId : null;
        }

        private async Task<Guid?> GetCurrentProjectIdAsync()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return null;
            }

            var student = await _unitOfWork.Students.GetByIdAsync(userId.Value);
            return student?.ProjectId;
        }
    }
}
