using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PfeManagement.Application.DTOs.UserStories;
using PfeManagement.Application.Interfaces;
using PfeManagement.Domain.Entities;
using PfeManagement.Domain.Interfaces;

namespace PfeManagement.Application.Services
{
    public class UserStoryService : IUserStoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserStoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<UserStoryResponseDto> CreateUserStoryAsync(CreateUserStoryDto dto)
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

            await _unitOfWork.UserStories.AddAsync(userStory);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(userStory);
        }

        public async Task<IEnumerable<UserStoryResponseDto>> GetUserStoriesAsync(Guid sprintId)
        {
            var userStories = await _unitOfWork.UserStories.GetAsync(us => us.SprintId == sprintId);
            return userStories.OrderBy(us => us.DueDate).Select(MapToDto);
        }

        public async Task<UserStoryResponseDto> UpdateUserStoryAsync(Guid storyId, UpdateUserStoryDto dto)
        {
            var userStory = await _unitOfWork.UserStories.GetByIdAsync(storyId);
            if (userStory == null) throw new Exception("UserStory not found");

            if (dto.StoryName != null) userStory.StoryName = dto.StoryName;
            if (dto.Description != null) userStory.Description = dto.Description;
            if (dto.Priority.HasValue) userStory.Priority = dto.Priority.Value;
            if (dto.StoryPointEstimate.HasValue) userStory.StoryPointEstimate = dto.StoryPointEstimate.Value;
            if (dto.DueDate.HasValue) userStory.DueDate = NormalizeUtc(dto.DueDate.Value);

            await _unitOfWork.UserStories.UpdateAsync(userStory);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(userStory);
        }

        public async Task DeleteUserStoryAsync(Guid storyId)
        {
            var userStory = await _unitOfWork.UserStories.GetByIdAsync(storyId);
            if (userStory == null) throw new Exception("UserStory not found");

            await _unitOfWork.UserStories.DeleteAsync(userStory);
            await _unitOfWork.SaveChangesAsync();
        }

        private static UserStoryResponseDto MapToDto(UserStory us)
        {
            return new UserStoryResponseDto
            {
                Id = us.Id,
                StoryName = us.StoryName,
                Description = us.Description,
                Priority = us.Priority,
                StoryPointEstimate = us.StoryPointEstimate,
                StartDate = us.StartDate,
                DueDate = us.DueDate,
                SprintId = us.SprintId
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
