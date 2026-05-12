using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PfeManagement.Application.DTOs.Tasks;
using PfeManagement.Application.Interfaces;
using PfeManagement.Domain.Entities;
using PfeManagement.Domain.Enums;
using PfeManagement.Domain.Events;
using PfeManagement.Domain.Exceptions;
using PfeManagement.Domain.Interfaces;

namespace PfeManagement.Application.Services
{
    public class TaskService : ITaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDomainEventDispatcher _eventDispatcher;

        public TaskService(IUnitOfWork unitOfWork, IDomainEventDispatcher eventDispatcher)
        {
            _unitOfWork = unitOfWork;
            _eventDispatcher = eventDispatcher;
        }

        public async Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto dto, Guid createdByUserId)
        {
            if (dto.Status != TaskItemStatus.ToDo)
            {
                throw new DomainConstraintException(
                    "New tasks must be created in ToDo status; use workflow transitions to change status.");
            }

            var userStory = await _unitOfWork.UserStories.GetByIdAsync(dto.UserStoryId);
            if (userStory == null) throw new Exception("UserStory not found");

            var task = userStory.AddTask(
                dto.Title,
                dto.Description,
                dto.Priority,
                dto.AssignedToId);

            await _unitOfWork.SaveChangesAsync();

            await _eventDispatcher.DispatchAsync(
                new TaskCreatedEvent(task.Id, task.Title, task.AssignedToId, task.UserStoryId, createdByUserId)
            );

            await _unitOfWork.SaveChangesAsync();

            return MapToDto(task);
        }

        public async Task<IEnumerable<TaskResponseDto>> GetTasksAsync(Guid userStoryId)
        {
            var tasks = await _unitOfWork.Tasks.GetAsync(t => t.UserStoryId == userStoryId);
            return tasks.Select(MapToDto);
        }

        public async Task<TaskResponseDto> UpdateTaskAsync(Guid taskId, UpdateTaskDto dto, Guid modifiedByUserId)
        {
            var task = await _unitOfWork.Tasks.GetByIdAsync(taskId);
            if (task == null) throw new Exception("Task not found");

            var oldStatus = task.Status;

            if (dto.Title != null) task.Title = dto.Title;
            if (dto.Description != null) task.Description = dto.Description;
            if (dto.Priority.HasValue) task.Priority = dto.Priority.Value;
            if (dto.AssignedToId.HasValue) task.AssignedToId = dto.AssignedToId.Value;

            if (dto.Status.HasValue && dto.Status.Value != task.Status)
            {
                task.MoveTo(dto.Status.Value);

                await _eventDispatcher.DispatchAsync(
                    new TaskStatusChangedEvent(task.Id, oldStatus, task.Status, modifiedByUserId)
                );
            }

            await _unitOfWork.Tasks.UpdateAsync(task);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(task);
        }

        public async Task DeleteTaskAsync(Guid taskId)
        {
            var task = await _unitOfWork.Tasks.GetByIdAsync(taskId);
            if (task == null) throw new Exception("Task not found");

            await _unitOfWork.Tasks.DeleteAsync(task);
            await _unitOfWork.SaveChangesAsync();
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
    }
}
