using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PfeManagement.Application.DTOs.Projects;
using PfeManagement.Application.Interfaces;
using PfeManagement.Domain.Entities;
using PfeManagement.Domain.Interfaces;

namespace PfeManagement.Application.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProjectService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ProjectResponseDto> CreateProjectAsync(CreateProjectDto dto, Guid studentId)
        {
            // Fetch the student creating the project
            var student = await _unitOfWork.Students.GetByIdAsync(studentId);
            if (student == null) throw new Exception("Student not found");
            if (student.ProjectId != null) throw new Exception("Student is already part of a project");

            var project = new Project
            {
                Title = dto.Title,
                Description = dto.Description,
                StartDate = NormalizeUtc(dto.StartDate),
                EndDate = NormalizeUtc(dto.EndDate)
            };

            // Add the creator as the first contributor
            project.Contributors.Add(student);

            // If dto.Contributors is provided, map them
            foreach (var contributorId in dto.Contributors)
            {
                var contributor = await _unitOfWork.Students.GetByIdAsync(contributorId);
                if (contributor != null && contributor.Id != studentId && contributor.ProjectId == null)
                {
                    project.Contributors.Add(contributor);
                }
            }

            await _unitOfWork.Projects.AddAsync(project);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(project);
        }

        public async Task<ProjectResponseDto> GetProjectAsync(Guid projectId)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
            if (project == null) throw new Exception("Project not found");
            return MapToDto(project);
        }

        public async Task<ProjectResponseDto> UpdateProjectAsync(Guid projectId, UpdateProjectDto dto)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
            if (project == null) throw new Exception("Project not found");

            if (dto.Title != null) project.Title = dto.Title;
            if (dto.Description != null) project.Description = dto.Description;
            if (dto.StartDate.HasValue) project.StartDate = NormalizeUtc(dto.StartDate.Value);
            if (dto.EndDate.HasValue) project.EndDate = NormalizeUtc(dto.EndDate.Value);

            await _unitOfWork.Projects.UpdateAsync(project);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(project);
        }

        public async Task DeleteProjectAsync(Guid projectId)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
            if (project == null) throw new Exception("Project not found");

            await _unitOfWork.Projects.DeleteAsync(project);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<ContributorDto>> GetStudentsWithoutProjectAsync()
        {
            var students = await _unitOfWork.Students.GetAsync(s => s.ProjectId == null);
            return students.Select(s => new ContributorDto
            {
                Id = s.Id,
                FullName = s.FullName,
                Email = s.Email
            });
        }

        public async Task AddContributorsAsync(Guid projectId, AddRemoveContributorsDto dto, Guid requesterId)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
            if (project == null) throw new Exception("Project not found");

            foreach (var studentId in dto.StudentIds)
            {
                var student = await _unitOfWork.Students.GetByIdAsync(studentId);
                if (student != null && student.ProjectId == null)
                {
                    student.ProjectId = projectId;
                    project.Contributors.Add(student);
                    await _unitOfWork.Students.UpdateAsync(student);
                }
            }
            await _unitOfWork.Projects.UpdateAsync(project);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RemoveContributorsAsync(Guid projectId, AddRemoveContributorsDto dto, Guid requesterId)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
            if (project == null) throw new Exception("Project not found");

            foreach (var studentId in dto.StudentIds)
            {
                var student = await _unitOfWork.Students.GetByIdAsync(studentId);
                if (student != null && student.ProjectId == projectId)
                {
                    student.ProjectId = null;
                    project.Contributors.Remove(student);
                    await _unitOfWork.Students.UpdateAsync(student);
                }
            }
            await _unitOfWork.Projects.UpdateAsync(project);
            await _unitOfWork.SaveChangesAsync();
        }

        private static ProjectResponseDto MapToDto(Project project)
        {
            return new ProjectResponseDto
            {
                ProjectId = project.Id,
                Title = project.Title,
                Description = project.Description,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                Contributors = project.Contributors.Select(c => new ContributorDto
                {
                    Id = c.Id,
                    FullName = c.FullName,
                    Email = c.Email
                }).ToList()
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
