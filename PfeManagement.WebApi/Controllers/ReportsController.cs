using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PfeManagement.Application.Interfaces;
using PfeManagement.Domain.Interfaces;
using PfeManagement.Infrastructure.Services; // FileStorage abstraction

namespace PfeManagement.WebApi.Controllers
{
    [ApiController]
    [Route("api/report")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IUnitOfWork _unitOfWork;

        public ReportsController(IReportService reportService, IFileStorageService fileStorageService, IUnitOfWork unitOfWork)
        {
            _reportService = reportService;
            _fileStorageService = fileStorageService;
            _unitOfWork = unitOfWork;
        }

        [Authorize]
        [HttpPost]
        [HttpPost("upload/{projectId}")]
        public async Task<IActionResult> CreateReport([FromRoute] Guid? projectId, [FromForm] IFormFile file, [FromForm] string notes)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { success = false, message = "No file uploaded." });
            }

            try
            {
                var effectiveProjectId = projectId ?? await GetCurrentProjectIdAsync();
                if (!effectiveProjectId.HasValue)
                {
                    return NotFound(new { success = false, message = "Project not found for current user" });
                }

                // Usage of IFileStorageService to abstract hard drive access
                using var stream = file.OpenReadStream();
                var filePath = await _fileStorageService.SaveFileAsync(stream, file.FileName);

                var result = await _reportService.ProcessReportAsync(effectiveProjectId.Value, filePath, notes);
                return StatusCode(201, new { success = true, message = "Report created successfully", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllReports([FromQuery] Guid? projectId)
        {
            var effectiveProjectId = projectId ?? await GetCurrentProjectIdAsync();
            if (!effectiveProjectId.HasValue)
            {
                return Ok(new { success = true, message = "Reports fetched successfully", data = Array.Empty<object>() });
            }

            var reports = await _unitOfWork.Reports.GetAsync(r => r.ProjectId == effectiveProjectId.Value);
            var data = reports.OrderByDescending(r => r.CreatedAt).Select(MapReport);
            return Ok(new { success = true, message = "Reports fetched successfully", data });
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReportById(Guid id)
        {
            var report = await _unitOfWork.Reports.GetByIdAsync(id);
            if (report == null)
            {
                return NotFound(new { success = false, message = "Report not found" });
            }

            return Ok(new { success = true, message = "Report fetched successfully", data = MapReport(report) });
        }

        [Authorize]
        [HttpGet("companysup/{projectID}")]
        [HttpGet("unisup/{projectID}")]
        public async Task<IActionResult> GetSupervisorReports(Guid projectID)
        {
            var reports = await _unitOfWork.Reports.GetAsync(r => r.ProjectId == projectID);
            var data = reports.OrderByDescending(r => r.CreatedAt).Select(MapReport);
            return Ok(new { success = true, message = "Reports fetched successfully", data });
        }

        [Authorize]
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateReport(Guid id, [FromBody] UpdateReportRequest request)
        {
            var report = await _unitOfWork.Reports.GetByIdAsync(id);
            if (report == null)
            {
                return NotFound(new { success = false, message = "Report not found" });
            }

            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                report.Notes = request.Notes;
            }

            await _unitOfWork.Reports.UpdateAsync(report);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { success = true, message = "Report updated successfully", data = MapReport(report) });
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReport(Guid id)
        {
            var report = await _unitOfWork.Reports.GetByIdAsync(id);
            if (report == null)
            {
                return NotFound(new { success = false, message = "Report not found" });
            }

            await _unitOfWork.Reports.DeleteAsync(report);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { success = true, message = "Report deleted successfully" });
        }

        private Guid? GetCurrentUserId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(sub, out var userId) ? userId : null;
        }

        private async Task<Guid?> GetCurrentProjectIdAsync()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return null;
            }

            var student = await _unitOfWork.Students.GetByIdAsync(userId.Value);
            return student?.ProjectId;
        }

        private static object MapReport(Domain.Entities.Report report)
        {
            return new
            {
                id = report.Id,
                versionLabel = report.VersionLabel,
                notes = report.Notes,
                filePath = report.FilePath,
                projectId = report.ProjectId,
                createdAt = report.CreatedAt
            };
        }

        public class UpdateReportRequest
        {
            public string? Notes { get; set; }
        }
    }
}
