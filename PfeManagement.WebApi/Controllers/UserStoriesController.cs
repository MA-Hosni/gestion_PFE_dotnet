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
    [Route("api/user-story")]
    public class UserStoriesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public UserStoriesController(AppDbContext db)
        {
            _db = db;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateUserStory([FromBody] CreateUserStoryDto dto)
        {
            try
            {
                var userStory = new UserStory
                {
                    StoryName = dto.StoryName,
                    Description = dto.Description,
                    Priority = dto.Priority,
                    StoryPointEstimate = dto.StoryPointEstimate,
                    DueDate = NormalizeUtc(dto.DueDate),
                    StartDate = NormalizeUtc(DateTime.UtcNow),
                    SprintId = dto.SprintId
                };

                _db.UserStories.Add(userStory);
                await _db.SaveChangesAsync();

                return StatusCode(201, new { success = true, message = "User story created successfully", data = MapToDto(userStory) });
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
                return Ok(new { success = true, message = "User stories fetched successfully", data = Array.Empty<object>() });

            var sprintIds = await _db.Sprints.Where(s => s.ProjectId == projectId.Value).Select(s => s.Id).ToListAsync();
            var stories = await _db.UserStories.Where(us => sprintIds.Contains(us.SprintId)).ToListAsync();

            var data = stories.Select(us => new
            {
                id = us.Id, storyName = us.StoryName, description = us.Description,
                priority = us.Priority, storyPointEstimate = us.StoryPointEstimate,
                startDate = us.StartDate, dueDate = us.DueDate, sprintId = us.SprintId
            });

            return Ok(new { success = true, message = "User stories fetched successfully", data });
        }

        [Authorize]
        [HttpGet("sprint/{sprintId}")]
        public async Task<IActionResult> GetUserStoriesForSprint(Guid sprintId)
        {
            var userStories = await _db.UserStories.Where(us => us.SprintId == sprintId).OrderBy(us => us.DueDate).ToListAsync();
            return Ok(new { success = true, message = "User stories fetched successfully", data = userStories.Select(MapToDto) });
        }

        [Authorize]
        [HttpGet("{userStoryId}")]
        public async Task<IActionResult> GetUserStoryById(Guid userStoryId)
        {
            var story = await _db.UserStories.FindAsync(userStoryId);
            if (story == null) return NotFound(new { success = false, message = "User story not found" });

            return Ok(new
            {
                success = true, message = "User story fetched successfully",
                data = new { id = story.Id, storyName = story.StoryName, description = story.Description,
                    priority = story.Priority, storyPointEstimate = story.StoryPointEstimate,
                    startDate = story.StartDate, dueDate = story.DueDate, sprintId = story.SprintId }
            });
        }

        [Authorize]
        [HttpPut("{userStoryId}")]
        public async Task<IActionResult> UpdateUserStory(Guid userStoryId, [FromBody] UpdateUserStoryDto dto)
        {
            try
            {
                var us = await _db.UserStories.FindAsync(userStoryId);
                if (us == null) throw new Exception("UserStory not found");
                if (dto.StoryName != null) us.StoryName = dto.StoryName;
                if (dto.Description != null) us.Description = dto.Description;
                if (dto.Priority.HasValue) us.Priority = dto.Priority.Value;
                if (dto.StoryPointEstimate.HasValue) us.StoryPointEstimate = dto.StoryPointEstimate.Value;
                if (dto.DueDate.HasValue) us.DueDate = NormalizeUtc(dto.DueDate.Value);
                await _db.SaveChangesAsync();
                return Ok(new { success = true, message = "User story updated successfully", data = MapToDto(us) });
            }
            catch (Exception ex) { return BadRequest(new { success = false, message = ex.Message }); }
        }

        [Authorize]
        [HttpDelete("{userStoryId}")]
        public async Task<IActionResult> DeleteUserStory(Guid userStoryId)
        {
            try
            {
                var us = await _db.UserStories.FindAsync(userStoryId);
                if (us == null) throw new Exception("UserStory not found");
                _db.UserStories.Remove(us);
                await _db.SaveChangesAsync();
                return Ok(new { success = true, message = "User story deleted successfully" });
            }
            catch (Exception ex) { return BadRequest(new { success = false, message = ex.Message }); }
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
            var s = await _db.Students.FindAsync(uid.Value);
            return s?.ProjectId;
        }

        private static UserStoryResponseDto MapToDto(UserStory us) => new()
        {
            Id = us.Id, StoryName = us.StoryName, Description = us.Description,
            Priority = us.Priority, StoryPointEstimate = us.StoryPointEstimate,
            StartDate = us.StartDate, DueDate = us.DueDate, SprintId = us.SprintId
        };

        private static DateTime NormalizeUtc(DateTime v) => v.Kind switch
        {
            DateTimeKind.Utc => v, DateTimeKind.Local => v.ToUniversalTime(),
            _ => DateTime.SpecifyKind(v, DateTimeKind.Utc)
        };
    }
}
