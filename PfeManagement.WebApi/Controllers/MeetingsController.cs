using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PfeManagement.WebApi.Data;
using PfeManagement.WebApi.Models;

namespace PfeManagement.WebApi.Controllers
{
    [ApiController]
    [Route("api/meetings")]
    public class MeetingsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public MeetingsController(AppDbContext db)
        {
            _db = db;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateMeeting([FromBody] CreateMeetingDto dto)
        {
            try
            {
                var creatorId = GetCurrentUserId() ?? Guid.Empty;
                var project = await _db.Projects.FindAsync(dto.ProjectId);
                if (project == null) throw new Exception("Project not found");

                var meeting = new Meeting
                {
                    ScheduledDate = NormalizeUtc(dto.ScheduledDate),
                    Agenda = dto.Agenda,
                    ReferenceType = dto.ReferenceType,
                    ReferenceId = dto.ReferenceId,
                    CreatedById = creatorId,
                    ProjectId = dto.ProjectId
                };

                _db.Meetings.Add(meeting);
                await _db.SaveChangesAsync();

                return StatusCode(201, new { success = true, message = "Meeting created successfully", data = MapMeeting(meeting) });
            }
            catch (Exception ex) { return BadRequest(new { success = false, message = ex.Message }); }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMeeting(Guid id, [FromBody] UpdateMeetingDto dto)
        {
            var meeting = await _db.Meetings.FindAsync(id);
            if (meeting == null) return NotFound(new { success = false, message = "Meeting not found" });
            if (!string.IsNullOrWhiteSpace(dto.ActualMinutes)) meeting.ActualMinutes = dto.ActualMinutes;
            await _db.SaveChangesAsync();
            return Ok(new { success = true, message = "Meeting updated successfully", data = MapMeeting(meeting) });
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMeeting(Guid id)
        {
            var meeting = await _db.Meetings.FindAsync(id);
            if (meeting == null) return NotFound(new { success = false, message = "Meeting not found" });
            _db.Meetings.Remove(meeting);
            await _db.SaveChangesAsync();
            return Ok(new { success = true, message = "Meeting deleted successfully" });
        }

        [Authorize]
        [HttpPatch("status/{id}")]
        [HttpPatch("validate/{id}")]
        public async Task<IActionResult> UpdateMeetingValidationStatus(Guid id, [FromBody] UpdateMeetingStatusRequest request)
        {
            var meeting = await _db.Meetings.FindAsync(id);
            if (meeting == null) return NotFound(new { success = false, message = "Meeting not found" });
            meeting.ValidationStatus = request.Status;
            meeting.ValidatorId = GetCurrentUserId();
            await _db.SaveChangesAsync();
            return Ok(new { success = true, message = "Meeting status updated", data = MapMeeting(meeting) });
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMeetingById(Guid id)
        {
            var meeting = await _db.Meetings.FindAsync(id);
            if (meeting == null) return NotFound(new { success = false, message = "Meeting not found" });
            return Ok(new { success = true, message = "Meeting fetched successfully", data = MapMeeting(meeting) });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetMeetings([FromQuery] Guid? projectId)
        {
            var meetings = projectId.HasValue
                ? await _db.Meetings.Where(m => m.ProjectId == projectId.Value).ToListAsync()
                : await _db.Meetings.ToListAsync();
            return Ok(new { success = true, message = "Meetings fetched successfully", data = meetings.OrderByDescending(m => m.ScheduledDate) });
        }

        private static object MapMeeting(Meeting m) => new
        {
            id = m.Id, scheduledDate = m.ScheduledDate, agenda = m.Agenda,
            actualMinutes = m.ActualMinutes, referenceType = m.ReferenceType,
            referenceId = m.ReferenceId, createdById = m.CreatedById,
            validationStatus = m.ValidationStatus, validatorId = m.ValidatorId,
            projectId = m.ProjectId, createdAt = m.CreatedAt, updatedAt = m.UpdatedAt
        };

        private Guid? GetCurrentUserId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(sub, out var id) ? id : null;
        }

        private static DateTime NormalizeUtc(DateTime v) => v.Kind switch
        {
            DateTimeKind.Utc => v, DateTimeKind.Local => v.ToUniversalTime(),
            _ => DateTime.SpecifyKind(v, DateTimeKind.Utc)
        };

        public class UpdateMeetingStatusRequest
        {
            public ValidationStatus Status { get; set; }
        }
    }
}
