using System;
using System.Threading.Tasks;
using PfeManagement.Application.DTOs.Tasks;

namespace PfeManagement.Application.Interfaces
{
    public interface ITaskReportService
    {
        Task<ProjectTaskReportDto> GetProjectTaskReportAsync(Guid projectId);
        Task<SprintTaskReportDto> GetSprintTaskReportAsync(Guid sprintId);
    }
}
