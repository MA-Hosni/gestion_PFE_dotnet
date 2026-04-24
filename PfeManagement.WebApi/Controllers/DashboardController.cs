using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PfeManagement.WebApi.Data;
using PfeManagement.WebApi.Models;

namespace PfeManagement.WebApi.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _db;

        public DashboardController(AppDbContext db)
        {
            _db = db;
        }

        [Authorize]
        [HttpGet("projects")]
        public async Task<IActionResult> GetAllProjects()
        {
            var projects = await _db.Projects.ToListAsync();
            return Ok(new { success = true, message = "Projects fetched successfully", data = projects });
        }

        [Authorize]
        [HttpGet("student/timeline")]
        public async Task<IActionResult> GetStudentTimeline()
        {
            var projectId = await GetCurrentProjectIdAsync();
            if (!projectId.HasValue)
                return Ok(new { success = true, message = "Timeline fetched successfully", data = Array.Empty<object>() });

            var sprints = await _db.Sprints.Where(s => s.ProjectId == projectId.Value).OrderBy(s => s.StartDate).ToListAsync();
            return Ok(new { success = true, message = "Timeline fetched successfully", data = sprints });
        }

        [Authorize]
        [HttpGet("student/tasks/standby")]
        public async Task<IActionResult> GetStudentStandbyTasks()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized(new { success = false, message = "User not authenticated" });

            var tasks = await _db.Tasks.Where(t => t.AssignedToId == userId.Value && t.Status == TaskItemStatus.ToDo).ToListAsync();
            return Ok(new { success = true, message = "Standby tasks fetched successfully", data = tasks });
        }

        [Authorize]
        [HttpGet("{projectId}")]
        public async Task<IActionResult> GetProgress(Guid projectId)
        {
            var sprintIds = await _db.Sprints.Where(s => s.ProjectId == projectId).Select(s => s.Id).ToListAsync();
            var storyIds = await _db.UserStories.Where(us => sprintIds.Contains(us.SprintId)).Select(us => us.Id).ToListAsync();
            var tasks = await _db.Tasks.Where(t => storyIds.Contains(t.UserStoryId)).ToListAsync();

            var data = new
            {
                projectId,
                totalTasks = tasks.Count,
                completedTasks = tasks.Count(t => t.Status == TaskItemStatus.Done),
                completionRate = tasks.Count == 0 ? 0 : Math.Round(tasks.Count(t => t.Status == TaskItemStatus.Done) * 100.0 / tasks.Count, 2)
            };

            return Ok(new { success = true, message = "Progress fetched successfully", data });
        }

        [Authorize]
        [HttpGet("supervisor/timeline")]
        public async Task<IActionResult> GetSupervisorTimeline()
        {
            var meetings = await _db.Meetings.OrderByDescending(m => m.ScheduledDate).ToListAsync();
            return Ok(new { success = true, message = "Supervisor timeline fetched successfully", data = meetings });
        }

        [Authorize]
        [HttpGet("supervisor/validations/{projectId}")]
        public async Task<IActionResult> GetSupervisorPendingValidations(Guid projectId)
        {
            var sprintIds = await _db.Sprints.Where(s => s.ProjectId == projectId).Select(s => s.Id).ToListAsync();
            var storyIds = await _db.UserStories.Where(us => sprintIds.Contains(us.SprintId)).Select(us => us.Id).ToListAsync();
            var taskIds = await _db.Tasks.Where(t => storyIds.Contains(t.UserStoryId)).Select(t => t.Id).ToListAsync();
            var pending = await _db.Validations.Where(v => taskIds.Contains(v.TaskId) && v.Status == ValidationStatus.Pending).ToListAsync();
            return Ok(new { success = true, message = "Pending validations fetched successfully", data = pending });
        }

        [Authorize]
        [HttpGet("supervisor/meetings/{projectId}")]
        public async Task<IActionResult> GetSupervisorLatestMeetings(Guid projectId)
        {
            var meetings = await _db.Meetings.Where(m => m.ProjectId == projectId).OrderByDescending(m => m.ScheduledDate).Take(10).ToListAsync();
            return Ok(new { success = true, message = "Latest meetings fetched successfully", data = meetings });
        }

        private Guid? GetCurrentUserId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(sub, out var id) ? id : null;
        }

        private async Task<Guid?> GetCurrentProjectIdAsync()
        {
            var uid = GetCurrentUserId();
            if (!uid.HasValue) return null;
            var student = await _db.Students.FindAsync(uid.Value);
            return student?.ProjectId;
        }
    }
}
