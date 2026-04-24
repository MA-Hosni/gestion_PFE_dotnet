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
    [Route("api/project/sprints")]
    public class SprintsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public SprintsController(AppDbContext db)
        {
            _db = db;
        }

        [Authorize]
        [HttpPost]
        [HttpPost("project/{projectId}")]
        public async Task<IActionResult> CreateSprint([FromRoute] Guid? projectId, [FromBody] CreateSprintDto dto)
        {
            try
            {
                var effectiveProjectId = projectId ?? await GetCurrentProjectIdAsync();
                if (!effectiveProjectId.HasValue)
                {
                    return NotFound(new { success = false, message = "Project not found for current user" });
                }

                var project = await _db.Projects.FindAsync(effectiveProjectId.Value);
                if (project == null) throw new Exception("Project not found");

                var existingSprints = await _db.Sprints.Where(s => s.ProjectId == effectiveProjectId.Value).ToListAsync();
                var nextIndex = existingSprints.Any() ? existingSprints.Max(s => s.OrderIndex) + 1 : 1;

                var sprint = new Sprint
                {
                    Title = dto.Title,
                    Goal = dto.Goal,
                    StartDate = NormalizeUtc(dto.StartDate),
                    EndDate = NormalizeUtc(dto.EndDate),
                    OrderIndex = nextIndex,
                    ProjectId = effectiveProjectId.Value
                };

                _db.Sprints.Add(sprint);
                await _db.SaveChangesAsync();

                return StatusCode(201, new { success = true, message = "Sprint created successfully", data = MapToDto(sprint) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet]
        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetSprintsForProject([FromRoute] Guid? projectId)
        {
            try
            {
                var effectiveProjectId = projectId ?? await GetCurrentProjectIdAsync();
                if (!effectiveProjectId.HasValue)
                {
                    return NotFound(new { success = false, message = "Project not found for current user" });
                }

                var sprints = await _db.Sprints.Where(s => s.ProjectId == effectiveProjectId.Value).OrderBy(s => s.OrderIndex).ToListAsync();
                return Ok(new { success = true, message = "Sprints fetched successfully", data = sprints.Select(MapToDto) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpPatch("{id}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSprint(Guid id, [FromBody] UpdateSprintDto dto)
        {
            try
            {
                var sprint = await _db.Sprints.FindAsync(id);
                if (sprint == null) throw new Exception("Sprint not found");

                if (dto.Title != null) sprint.Title = dto.Title;
                if (dto.Goal != null) sprint.Goal = dto.Goal;
                if (dto.StartDate.HasValue) sprint.StartDate = NormalizeUtc(dto.StartDate.Value);
                if (dto.EndDate.HasValue) sprint.EndDate = NormalizeUtc(dto.EndDate.Value);

                await _db.SaveChangesAsync();

                return Ok(new { success = true, message = "Sprint updated successfully", data = MapToDto(sprint) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSprint(Guid id)
        {
            try
            {
                var sprint = await _db.Sprints.FindAsync(id);
                if (sprint == null) throw new Exception("Sprint not found");

                _db.Sprints.Remove(sprint);
                await _db.SaveChangesAsync();

                return Ok(new { success = true, message = "Sprint deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpPatch("reorder")]
        [HttpPost("reorder/project/{projectId}")]
        public async Task<IActionResult> ReorderSprints([FromRoute] Guid? projectId, [FromBody] ReorderSprintsDto dto)
        {
            try
            {
                var effectiveProjectId = projectId ?? await GetCurrentProjectIdAsync();
                if (!effectiveProjectId.HasValue)
                {
                    return NotFound(new { success = false, message = "Project not found for current user" });
                }

                foreach (var sprintOrder in dto.Sprints)
                {
                    var sprint = await _db.Sprints.FindAsync(sprintOrder.SprintId);
                    if (sprint != null && sprint.ProjectId == effectiveProjectId.Value)
                    {
                        sprint.OrderIndex = sprintOrder.OrderIndex;
                    }
                }
                await _db.SaveChangesAsync();

                return Ok(new { success = true, message = "Sprints reordered successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        private Guid? GetCurrentUserId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(sub, out var userId) ? userId : null;
        }

        private async Task<Guid?> GetCurrentProjectIdAsync()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return null;
            var student = await _db.Students.FindAsync(userId.Value);
            return student?.ProjectId;
        }

        private static SprintResponseDto MapToDto(Sprint sprint)
        {
            return new SprintResponseDto
            {
                Id = sprint.Id,
                Title = sprint.Title,
                Goal = sprint.Goal,
                StartDate = sprint.StartDate,
                EndDate = sprint.EndDate,
                OrderIndex = sprint.OrderIndex
            };
        }

        private static DateTime NormalizeUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };
        }
    }
}
