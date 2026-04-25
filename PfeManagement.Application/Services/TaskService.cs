using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PfeManagement.Application.DTOs.Tasks;
using PfeManagement.Application.Interfaces;
using PfeManagement.Domain.Entities;
using PfeManagement.Domain.Events;
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

        public async Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto dto)
        {
            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                Status = dto.Status,
                Priority = dto.Priority,
                UserStoryId = dto.UserStoryId,
                AssignedToId = dto.AssignedToId
            };

            await _unitOfWork.Tasks.AddAsync(task);
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
            
            if (dto.Status.HasValue && dto.Status != task.Status)
            {
                task.Status = dto.Status.Value;
                // Dispatch domain event (Observer pattern)
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
