using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PfeManagement.Application.DTOs.UserStories;
using PfeManagement.Application.Interfaces;
using PfeManagement.Domain.Interfaces;

namespace PfeManagement.WebApi.Controllers
{
    [ApiController]
    [Route("api/user-story")]
    public class UserStoriesController : ControllerBase
    {
        private readonly IUserStoryService _userStoryService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProjectManagementFacade _projectManagementFacade;

        public UserStoriesController(
            IUserStoryService userStoryService,
            IUnitOfWork unitOfWork,
            IProjectManagementFacade projectManagementFacade)
        {
            _userStoryService = userStoryService;
            _unitOfWork = unitOfWork;
            _projectManagementFacade = projectManagementFacade;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateUserStory([FromBody] CreateUserStoryDto dto)
        {
            try
            {
                var result = await _userStoryService.CreateUserStoryAsync(dto);
                return StatusCode(201, new { success = true, message = "User story created successfully", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUserStories()
        {
            var projectId = await GetCurrentProjectIdAsync();
            if (!projectId.HasValue)
            {
                return Ok(new { success = true, message = "User stories fetched successfully", data = Array.Empty<object>() });
            }

            var stories = await _projectManagementFacade.GetUserStoriesForProjectAsync(projectId.Value);
            return Ok(new { success = true, message = "User stories fetched successfully", data = stories });
        }

        [Authorize]
        [HttpGet("sprint/{sprintId}")]
        public async Task<IActionResult> GetUserStoriesForSprint(Guid sprintId)
        {
            try
            {
                var result = await _userStoryService.GetUserStoriesAsync(sprintId);
                return Ok(new { success = true, message = "User stories fetched successfully", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("{userStoryId}")]
        public async Task<IActionResult> GetUserStoryById(Guid userStoryId)
        {
            var story = await _unitOfWork.UserStories.GetByIdAsync(userStoryId);
            if (story == null)
            {
                return NotFound(new { success = false, message = "User story not found" });
            }

            return Ok(new
            {
                success = true,
                message = "User story fetched successfully",
                data = new
                {
                    id = story.Id,
                    storyName = story.StoryName,
                    description = story.Description,
                    priority = story.Priority,
                    storyPointEstimate = story.StoryPointEstimate,
                    startDate = story.StartDate,
                    dueDate = story.DueDate,
                    sprintId = story.SprintId
                }
            });
        }

        [Authorize]
        [HttpPut("{userStoryId}")]
        public async Task<IActionResult> UpdateUserStory(Guid userStoryId, [FromBody] UpdateUserStoryDto dto)
        {
            try
            {
                var result = await _userStoryService.UpdateUserStoryAsync(userStoryId, dto);
                return Ok(new { success = true, message = "User story updated successfully", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("{userStoryId}")]
        public async Task<IActionResult> DeleteUserStory(Guid userStoryId)
        {
            try
            {
                await _userStoryService.DeleteUserStoryAsync(userStoryId);
                return Ok(new { success = true, message = "User story deleted successfully" });
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
