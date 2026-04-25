using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PfeManagement.Application.DTOs.Projects;
using PfeManagement.Application.Interfaces;
using PfeManagement.Domain.Interfaces;

namespace PfeManagement.WebApi.Controllers
{
    [ApiController]
    [Route("api/project")]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly IUnitOfWork _unitOfWork;

        public ProjectsController(IProjectService projectService, IUnitOfWork unitOfWork)
        {
            _projectService = projectService;
            _unitOfWork = unitOfWork;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto dto)
        {
            try
            {
                var studentId = await GetCurrentStudentIdAsync();
                if (!studentId.HasValue)
                {
                    return Unauthorized(new { success = false, message = "Student not authenticated" });
                }

                var result = await _projectService.CreateProjectAsync(dto, studentId.Value);
                return StatusCode(201, new { success = true, message = "Project created successfully", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject([FromRoute] Guid? id)
        {
            try
            {
                var projectId = id ?? await GetCurrentProjectIdAsync();
                if (!projectId.HasValue)
                {
                    return NotFound(new { success = false, message = "Project not found for current user" });
                }

                var result = await _projectService.GetProjectAsync(projectId.Value);
                return Ok(new { success = true, message = "Project fetched successfully", data = result });
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpPatch]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject([FromRoute] Guid? id, [FromBody] UpdateProjectDto dto)
        {
            try
            {
                var projectId = id ?? await GetCurrentProjectIdAsync();
                if (!projectId.HasValue)
                {
                    return NotFound(new { success = false, message = "Project not found for current user" });
                }

                var result = await _projectService.UpdateProjectAsync(projectId.Value, dto);
                return Ok(new { success = true, message = "Project updated successfully", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject([FromRoute] Guid? id)
        {
            try
            {
                var projectId = id ?? await GetCurrentProjectIdAsync();
                if (!projectId.HasValue)
                {
                    return NotFound(new { success = false, message = "Project not found for current user" });
                }

                await _projectService.DeleteProjectAsync(projectId.Value);
                return Ok(new { success = true, message = "Project deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("students/without-project")]
        [HttpGet("available-students")]
        public async Task<IActionResult> GetAvailableStudents()
        {
            var result = await _projectService.GetStudentsWithoutProjectAsync();
            return Ok(new { success = true, message = "Students fetched successfully", data = result });
        }

        [Authorize]
        [HttpPatch("students/add-contributors")]
        [HttpPost("{id}/contributors")]
        public async Task<IActionResult> AddContributors([FromRoute] Guid? id, [FromBody] AddRemoveContributorsDto dto)
        {
            try
            {
                var requesterId = GetCurrentUserId();
                if (!requesterId.HasValue)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                var projectId = id ?? await GetCurrentProjectIdAsync();
                if (!projectId.HasValue)
                {
                    return NotFound(new { success = false, message = "Project not found for current user" });
                }

                await _projectService.AddContributorsAsync(projectId.Value, dto, requesterId.Value);
                return Ok(new { success = true, message = "Contributors added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpPatch("students/remove-contributors")]
        [HttpDelete("{id}/contributors")]
        public async Task<IActionResult> RemoveContributors([FromRoute] Guid? id, [FromBody] AddRemoveContributorsDto dto)
        {
            try
            {
                var requesterId = GetCurrentUserId();
                if (!requesterId.HasValue)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                var projectId = id ?? await GetCurrentProjectIdAsync();
                if (!projectId.HasValue)
                {
                    return NotFound(new { success = false, message = "Project not found for current user" });
                }

                await _projectService.RemoveContributorsAsync(projectId.Value, dto, requesterId.Value);
                return Ok(new { success = true, message = "Contributors removed successfully" });
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

        private async Task<Guid?> GetCurrentStudentIdAsync()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return null;
            }

            var student = await _unitOfWork.Students.GetByIdAsync(userId.Value);
            return student?.Id;
        }

        private async Task<Guid?> GetCurrentProjectIdAsync()
        {
            var studentId = await GetCurrentStudentIdAsync();
            if (!studentId.HasValue)
            {
                return null;
            }

            var student = await _unitOfWork.Students.GetByIdAsync(studentId.Value);
            return student?.ProjectId;
        }
    }
}
