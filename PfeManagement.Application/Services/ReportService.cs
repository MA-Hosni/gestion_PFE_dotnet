using System;
using System.Threading.Tasks;
using PfeManagement.Application.DTOs.Reports;
using PfeManagement.Application.Interfaces;
using PfeManagement.Domain.Entities;
using PfeManagement.Domain.Interfaces;
// using statement removed to respect Dependency Inversion M

namespace PfeManagement.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;

        // Ignoring actual File I/O for brief DB-centric example
        public ReportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ReportResponseDto> ProcessReportAsync(Guid projectId, string filePath, string notes)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
            if (project == null) throw new Exception("Project not found");

            var report = new Report
            {
                VersionLabel = 1, // Can calculate based on existing
                Notes = notes,
                FilePath = filePath,
                ProjectId = projectId
            };

            await _unitOfWork.Reports.AddAsync(report);
            await _unitOfWork.SaveChangesAsync();

            return new ReportResponseDto
            {
                Id = report.Id,
                VersionLabel = report.VersionLabel,
                Notes = report.Notes,
                FilePath = report.FilePath,
                ProjectId = report.ProjectId
            };
        }
    }
}
