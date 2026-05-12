using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PfeManagement.Application.DTOs.Facades;
using PfeManagement.Application.Interfaces;

namespace PfeManagement.WebApi.Controllers
{
    /// <summary>
    /// Entry points for multi-step workflows exposed through the Project Management Facade.
    /// </summary>
    [ApiController]
    [Route("api/project/workflow")]
    public class ProjectWorkflowController : ControllerBase
    {
        private readonly IProjectManagementFacade _projectManagementFacade;

        public ProjectWorkflowController(IProjectManagementFacade projectManagementFacade)
        {
            _projectManagementFacade = projectManagementFacade;
        }

        [Authorize]
        [HttpPost("sprint-with-user-stories/{projectId:guid}")]
        public async Task<IActionResult> CreateSprintWithUserStories(
            Guid projectId,
            [FromBody] CreateSprintWithUserStoriesDto dto)
        {
            try
            {
                var result = await _projectManagementFacade.CreateSprintWithUserStoriesAsync(projectId, dto);
                return StatusCode(201, new { success = true, message = "Sprint and user stories created successfully", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("user-story-with-tasks")]
        public async Task<IActionResult> CreateUserStoryWithTasks([FromBody] CreateUserStoryWithTasksDto dto)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { success = false, message = "User not authenticated" });

            try
            {
                var result = await _projectManagementFacade.CreateUserStoryWithInitialTasksAsync(dto, userId.Value);
                return StatusCode(201, new { success = true, message = "User story and tasks created successfully", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("planning-overview/{projectId:guid}")]
        public async Task<IActionResult> GetPlanningOverview(Guid projectId)
        {
            try
            {
                var result = await _projectManagementFacade.GetProjectPlanningOverviewAsync(projectId);
                return Ok(new { success = true, message = "Planning overview fetched successfully", data = result });
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
    }
}
