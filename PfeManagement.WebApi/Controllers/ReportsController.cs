using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PfeManagement.WebApi.Data;
using PfeManagement.WebApi.Models;

namespace PfeManagement.WebApi.Controllers
{
    [ApiController]
    [Route("api/report")]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ReportsController(AppDbContext db)
        {
            _db = db;
        }

        [Authorize]
        [HttpPost]
        [HttpPost("upload/{projectId}")]
        public async Task<IActionResult> CreateReport([FromRoute] Guid? projectId, [FromForm] IFormFile file, [FromForm] string notes)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, message = "No file uploaded." });

            try
            {
                var effectiveProjectId = projectId ?? await GetCurrentProjectIdAsync();
                if (!effectiveProjectId.HasValue)
                    return NotFound(new { success = false, message = "Project not found for current user" });

                var project = await _db.Projects.FindAsync(effectiveProjectId.Value);
                if (project == null) throw new Exception("Project not found");

                // Inline file storage (no IFileStorageService)
                var uploadDir = "Uploads";
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);
                var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(uploadDir, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var report = new Report
                {
                    VersionLabel = 1,
                    Notes = notes,
                    FilePath = filePath,
                    ProjectId = effectiveProjectId.Value
                };

                _db.Reports.Add(report);
                await _db.SaveChangesAsync();

                return StatusCode(201, new { success = true, message = "Report created successfully", data = MapReport(report) });
            }
            catch (Exception ex) { return BadRequest(new { success = false, message = ex.Message }); }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllReports([FromQuery] Guid? projectId)
        {
            var effectiveProjectId = projectId ?? await GetCurrentProjectIdAsync();
            if (!effectiveProjectId.HasValue)
                return Ok(new { success = true, message = "Reports fetched successfully", data = Array.Empty<object>() });

            var reports = await _db.Reports.Where(r => r.ProjectId == effectiveProjectId.Value).OrderByDescending(r => r.CreatedAt).ToListAsync();
            return Ok(new { success = true, message = "Reports fetched successfully", data = reports.Select(MapReport) });
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReportById(Guid id)
        {
            var report = await _db.Reports.FindAsync(id);
            if (report == null) return NotFound(new { success = false, message = "Report not found" });
            return Ok(new { success = true, message = "Report fetched successfully", data = MapReport(report) });
        }

        [Authorize]
        [HttpGet("companysup/{projectID}")]
        [HttpGet("unisup/{projectID}")]
        public async Task<IActionResult> GetSupervisorReports(Guid projectID)
        {
            var reports = await _db.Reports.Where(r => r.ProjectId == projectID).OrderByDescending(r => r.CreatedAt).ToListAsync();
            return Ok(new { success = true, message = "Reports fetched successfully", data = reports.Select(MapReport) });
        }

        [Authorize]
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateReport(Guid id, [FromBody] UpdateReportRequest request)
        {
            var report = await _db.Reports.FindAsync(id);
            if (report == null) return NotFound(new { success = false, message = "Report not found" });
            if (!string.IsNullOrWhiteSpace(request.Notes)) report.Notes = request.Notes;
            await _db.SaveChangesAsync();
            return Ok(new { success = true, message = "Report updated successfully", data = MapReport(report) });
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReport(Guid id)
        {
            var report = await _db.Reports.FindAsync(id);
            if (report == null) return NotFound(new { success = false, message = "Report not found" });
            _db.Reports.Remove(report);
            await _db.SaveChangesAsync();
            return Ok(new { success = true, message = "Report deleted successfully" });
        }

        private Guid? GetCurrentUserId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(sub, out var id) ? id : null;
        }

        private async Task<Guid?> GetCurrentProjectIdAsync()
        {
            var uid = GetCurrentUserId();
            if (!uid.HasValue) return null;
            var student = await _db.Students.FindAsync(uid.Value);
            return student?.ProjectId;
        }

        private static object MapReport(Report r) => new
        {
            id = r.Id, versionLabel = r.VersionLabel, notes = r.Notes,
            filePath = r.FilePath, projectId = r.ProjectId, createdAt = r.CreatedAt
        };

        public class UpdateReportRequest
        {
            public string? Notes { get; set; }
        }
    }
}
