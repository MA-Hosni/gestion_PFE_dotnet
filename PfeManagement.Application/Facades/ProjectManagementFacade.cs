using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PfeManagement.Application.DTOs.Facades;
using PfeManagement.Application.DTOs.Tasks;
using PfeManagement.Application.DTOs.UserStories;
using PfeManagement.Application.Interfaces;
using PfeManagement.Domain.Enums;
using PfeManagement.Domain.Interfaces;

namespace PfeManagement.Application.Facades
{
    /// <summary>
    /// Facade implementation: coordinates <see cref="ISprintService"/>, <see cref="IUserStoryService"/>,
    /// <see cref="ITaskService"/>, <see cref="ITaskReportService"/>, and read access via <see cref="IUnitOfWork"/>
    /// for workflows that would otherwise require multiple controller-level calls.
    /// </summary>
    public class ProjectManagementFacade : IProjectManagementFacade
    {
        private readonly ISprintService _sprintService;
        private readonly IUserStoryService _userStoryService;
        private readonly ITaskService _taskService;
        private readonly ITaskReportService _taskReportService;
        private readonly IUnitOfWork _unitOfWork;

        public ProjectManagementFacade(
            ISprintService sprintService,
            IUserStoryService userStoryService,
            ITaskService taskService,
            ITaskReportService taskReportService,
            IUnitOfWork unitOfWork)
        {
            _sprintService = sprintService;
            _userStoryService = userStoryService;
            _taskService = taskService;
            _taskReportService = taskReportService;
            _unitOfWork = unitOfWork;
        }

        public async Task<SprintPlanningWorkflowResultDto> CreateSprintWithUserStoriesAsync(
            Guid projectId,
            CreateSprintWithUserStoriesDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var sprint = await _sprintService.CreateSprintAsync(dto.Sprint, projectId);
            var createdStories = new List<UserStoryResponseDto>();

            foreach (var item in dto.UserStories ?? new List<CreateUserStoryForSprintItemDto>())
            {
                var storyDto = new CreateUserStoryDto
                {
                    StoryName = item.StoryName,
                    Description = item.Description,
                    Priority = item.Priority,
                    StoryPointEstimate = item.StoryPointEstimate,
                    DueDate = item.DueDate,
                    SprintId = sprint.Id
                };
                createdStories.Add(await _userStoryService.CreateUserStoryAsync(storyDto));
            }

            return new SprintPlanningWorkflowResultDto
            {
                Sprint = sprint,
                UserStories = createdStories
            };
        }

        public async Task<UserStoryWithTasksWorkflowResultDto> CreateUserStoryWithInitialTasksAsync(
            CreateUserStoryWithTasksDto dto,
            Guid createdByUserId)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var story = await _userStoryService.CreateUserStoryAsync(dto.UserStory);
            var tasks = new List<TaskResponseDto>();

            foreach (var seed in dto.InitialTasks ?? new List<InitialTaskItemDto>())
            {
                var taskDto = new CreateTaskDto
                {
                    Title = seed.Title,
                    Description = seed.Description,
                    Priority = seed.Priority,
                    AssignedToId = seed.AssignedToId,
                    UserStoryId = story.Id,
                    Status = TaskItemStatus.ToDo
                };
                tasks.Add(await _taskService.CreateTaskAsync(taskDto, createdByUserId));
            }

            return new UserStoryWithTasksWorkflowResultDto
            {
                UserStory = story,
                Tasks = tasks
            };
        }

        public async Task<IReadOnlyList<UserStoryPlanningSummaryDto>> GetUserStoriesForProjectAsync(Guid projectId)
        {
            var sprints = await _unitOfWork.Sprints.GetAsync(s => s.ProjectId == projectId);
            var sprintIds = sprints.Select(s => s.Id).ToHashSet();
            if (sprintIds.Count == 0)
                return Array.Empty<UserStoryPlanningSummaryDto>();

            var stories = await _unitOfWork.UserStories.GetAsync(us => sprintIds.Contains(us.SprintId));
            return stories
                .OrderBy(us => us.DueDate)
                .Select(us => new UserStoryPlanningSummaryDto
                {
                    Id = us.Id,
                    StoryName = us.StoryName,
                    Description = us.Description,
                    Priority = us.Priority,
                    StoryPointEstimate = us.StoryPointEstimate,
                    StartDate = us.StartDate,
                    DueDate = us.DueDate,
                    SprintId = us.SprintId
                })
                .ToList();
        }

        public async Task<ProjectPlanningOverviewDto> GetProjectPlanningOverviewAsync(Guid projectId)
        {
            var sprints = (await _sprintService.GetSprintsAsync(projectId)).ToList();
            var report = await _taskReportService.GetProjectTaskReportAsync(projectId);

            return new ProjectPlanningOverviewDto
            {
                Sprints = sprints,
                TaskReport = report
            };
        }
    }
}
