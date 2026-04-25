using System;
using System.Linq;
using System.Threading.Tasks;
using PfeManagement.Application.DTOs.Tasks;
using PfeManagement.Application.Interfaces;
using PfeManagement.Domain.Interfaces;

namespace PfeManagement.Application.Services
{
    public class TaskReportService : ITaskReportService
    {
        private readonly ITaskReportDataAccess _dataAccess;

        public TaskReportService(ITaskReportDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

        public async Task<ProjectTaskReportDto> GetProjectTaskReportAsync(Guid projectId)
        {
            var sprints = await _dataAccess.Sprints.GetAsync(s => s.ProjectId == projectId);
            var sprintIds = sprints.Select(s => s.Id).ToHashSet();
            
            var userStories = await _dataAccess.UserStories.GetAsync(us => sprintIds.Contains(us.SprintId));
            var storyIds = userStories.Select(us => us.Id).ToHashSet();
            
            var tasks = await _dataAccess.Tasks.GetAsync(t => storyIds.Contains(t.UserStoryId));

            return new ProjectTaskReportDto
            {
                ProjectId = projectId,
                TotalTasks = tasks.Count(),
                ByStatus = tasks.GroupBy(t => t.Status).ToDictionary(g => g.Key.ToString(), g => g.Count())
            };
        }

        public async Task<SprintTaskReportDto> GetSprintTaskReportAsync(Guid sprintId)
        {
            var userStories = await _dataAccess.UserStories.GetAsync(us => us.SprintId == sprintId);
            var storyIds = userStories.Select(us => us.Id).ToHashSet();
            
            var tasks = await _dataAccess.Tasks.GetAsync(t => storyIds.Contains(t.UserStoryId));

            return new SprintTaskReportDto
            {
                SprintId = sprintId,
                TotalTasks = tasks.Count(),
                ByStatus = tasks.GroupBy(t => t.Status).ToDictionary(g => g.Key.ToString(), g => g.Count())
            };
        }
    }
}
