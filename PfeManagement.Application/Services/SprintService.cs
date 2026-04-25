using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PfeManagement.Application.DTOs.Sprints;
using PfeManagement.Application.Interfaces;
using PfeManagement.Domain.Entities;
using PfeManagement.Domain.Interfaces;

namespace PfeManagement.Application.Services
{
    public class SprintService : ISprintService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SprintService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<SprintResponseDto> CreateSprintAsync(CreateSprintDto dto, Guid projectId)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
            if (project == null) throw new Exception("Project not found");

            // Calculate OrderIndex based on existing sprints
            var existingSprints = await _unitOfWork.Sprints.GetAsync(s => s.ProjectId == projectId);
            var nextIndex = existingSprints.Any() ? existingSprints.Max(s => s.OrderIndex) + 1 : 1;

            var sprint = new Sprint
            {
                Title = dto.Title,
                Goal = dto.Goal,
                StartDate = NormalizeUtc(dto.StartDate),
                EndDate = NormalizeUtc(dto.EndDate),
                OrderIndex = nextIndex,
                ProjectId = projectId
            };

            await _unitOfWork.Sprints.AddAsync(sprint);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(sprint);
        }

        public async Task<IEnumerable<SprintResponseDto>> GetSprintsAsync(Guid projectId)
        {
            var sprints = await _unitOfWork.Sprints.GetAsync(s => s.ProjectId == projectId);
            return sprints.OrderBy(s => s.OrderIndex).Select(MapToDto);
        }

        public async Task<SprintResponseDto> UpdateSprintAsync(Guid sprintId, UpdateSprintDto dto)
        {
            var sprint = await _unitOfWork.Sprints.GetByIdAsync(sprintId);
            if (sprint == null) throw new Exception("Sprint not found");

            if (dto.Title != null) sprint.Title = dto.Title;
            if (dto.Goal != null) sprint.Goal = dto.Goal;
            if (dto.StartDate.HasValue) sprint.StartDate = NormalizeUtc(dto.StartDate.Value);
            if (dto.EndDate.HasValue) sprint.EndDate = NormalizeUtc(dto.EndDate.Value);

            await _unitOfWork.Sprints.UpdateAsync(sprint);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(sprint);
        }

        public async Task DeleteSprintAsync(Guid sprintId)
        {
            var sprint = await _unitOfWork.Sprints.GetByIdAsync(sprintId);
            if (sprint == null) throw new Exception("Sprint not found");

            await _unitOfWork.Sprints.DeleteAsync(sprint);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ReorderSprintsAsync(ReorderSprintsDto dto, Guid projectId)
        {
            foreach (var sprintOrder in dto.Sprints)
            {
                var sprint = await _unitOfWork.Sprints.GetByIdAsync(sprintOrder.SprintId);
                if (sprint != null && sprint.ProjectId == projectId)
                {
                    sprint.OrderIndex = sprintOrder.OrderIndex;
                    await _unitOfWork.Sprints.UpdateAsync(sprint);
                }
            }
            await _unitOfWork.SaveChangesAsync();
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
