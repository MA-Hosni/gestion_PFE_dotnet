using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PfeManagement.Application.DTOs.Sprints;
using PfeManagement.Application.Interfaces;
using PfeManagement.Domain.Interfaces;

namespace PfeManagement.WebApi.Controllers
{
    [ApiController]
    [Route("api/project/sprints")]
    public class SprintsController : ControllerBase
    {
        private readonly ISprintService _sprintService;
        private readonly IUnitOfWork _unitOfWork;

        public SprintsController(ISprintService sprintService, IUnitOfWork unitOfWork)
        {
            _sprintService = sprintService;
            _unitOfWork = unitOfWork;
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

                var result = await _sprintService.CreateSprintAsync(dto, effectiveProjectId.Value);
                return StatusCode(201, new { success = true, message = "Sprint created successfully", data = result });
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

                var result = await _sprintService.GetSprintsAsync(effectiveProjectId.Value);
                return Ok(new { success = true, message = "Sprints fetched successfully", data = result });
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
            var result = await _sprintService.UpdateSprintAsync(id, dto);
                return Ok(new { success = true, message = "Sprint updated successfully", data = result });
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
            await _sprintService.DeleteSprintAsync(id);
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

                await _sprintService.ReorderSprintsAsync(dto, effectiveProjectId.Value);
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
            if (!userId.HasValue)
            {
                return null;
            }

            var student = await _unitOfWork.Students.GetByIdAsync(userId.Value);
            return student?.ProjectId;
        }
    }
}
