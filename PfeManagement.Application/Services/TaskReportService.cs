using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PfeManagement.Application.DTOs.Tasks;
using PfeManagement.Application.Interfaces;
using PfeManagement.Domain.Interfaces;

namespace PfeManagement.Application.Services
{
    public class TaskReportService : ITaskReportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TaskReportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ProjectTaskReportDto> GetProjectTaskReportAsync(Guid projectId)
        {
            var sprints = await _unitOfWork.Sprints.GetAsync(s => s.ProjectId == projectId);
            var sprintIds = sprints.Select(s => s.Id).ToHashSet();
            if (!sprintIds.Any())
            {
                return new ProjectTaskReportDto { ProjectId = projectId, TotalTasks = 0, ByStatus = new Dictionary<string, int>() };
            }

            var userStories = await _unitOfWork.UserStories.GetAsync(us => sprintIds.Contains(us.SprintId));
            var storyIds = userStories.Select(us => us.Id).ToHashSet();
            if (!storyIds.Any())
            {
                return new ProjectTaskReportDto { ProjectId = projectId, TotalTasks = 0, ByStatus = new Dictionary<string, int>() };
            }

            var tasks = await _unitOfWork.Tasks.GetAsync(t => storyIds.Contains(t.UserStoryId));

            return new ProjectTaskReportDto
            {
                ProjectId = projectId,
                TotalTasks = tasks.Count,
                ByStatus = tasks.GroupBy(t => t.Status).ToDictionary(g => g.Key.ToString(), g => g.Count())
            };
        }

        public async Task<SprintTaskReportDto> GetSprintTaskReportAsync(Guid sprintId)
        {
            var userStories = await _unitOfWork.UserStories.GetAsync(us => us.SprintId == sprintId);
            var storyIds = userStories.Select(us => us.Id).ToHashSet();
            if (!storyIds.Any())
            {
                return new SprintTaskReportDto { SprintId = sprintId, TotalTasks = 0, ByStatus = new Dictionary<string, int>() };
            }

            var tasks = await _unitOfWork.Tasks.GetAsync(t => storyIds.Contains(t.UserStoryId));

            return new SprintTaskReportDto
            {
                SprintId = sprintId,
                TotalTasks = tasks.Count,
                ByStatus = tasks.GroupBy(t => t.Status).ToDictionary(g => g.Key.ToString(), g => g.Count())
            };
        }
    }
}
