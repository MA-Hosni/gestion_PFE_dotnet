using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PfeManagement.Application.DTOs.Meetings;
using PfeManagement.Application.Interfaces;
using PfeManagement.Domain.Enums;
using PfeManagement.Domain.Interfaces;

namespace PfeManagement.WebApi.Controllers
{
    [ApiController]
    [Route("api/meetings")]
    public class MeetingsController : ControllerBase
    {
        private readonly IMeetingService _meetingService;
        private readonly IUnitOfWork _unitOfWork;

        public MeetingsController(IMeetingService meetingService, IUnitOfWork unitOfWork)
        {
            _meetingService = meetingService;
            _unitOfWork = unitOfWork;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateMeeting([FromBody] CreateMeetingDto dto)
        {
            try
            {
                var creatorId = GetCurrentUserId() ?? Guid.Empty;
                var result = await _meetingService.CreateMeetingAsync(dto, creatorId);
                return StatusCode(201, new { success = true, message = "Meeting created successfully", data = MapMeeting(result) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMeeting(Guid id, [FromBody] UpdateMeetingDto dto)
        {
            var meeting = await _unitOfWork.Meetings.GetByIdAsync(id);
            if (meeting == null)
            {
                return NotFound(new { success = false, message = "Meeting not found" });
            }

            if (!string.IsNullOrWhiteSpace(dto.ActualMinutes))
            {
                meeting.ActualMinutes = dto.ActualMinutes;
            }

            await _unitOfWork.Meetings.UpdateAsync(meeting);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { success = true, message = "Meeting updated successfully", data = MapMeeting(meeting) });
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMeeting(Guid id)
        {
            var meeting = await _unitOfWork.Meetings.GetByIdAsync(id);
            if (meeting == null)
            {
                return NotFound(new { success = false, message = "Meeting not found" });
            }

            await _unitOfWork.Meetings.DeleteAsync(meeting);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { success = true, message = "Meeting deleted successfully" });
        }

        [Authorize]
        [HttpPatch("status/{id}")]
        [HttpPatch("validate/{id}")]
        public async Task<IActionResult> UpdateMeetingValidationStatus(Guid id, [FromBody] UpdateMeetingStatusRequest request)
        {
            var meeting = await _unitOfWork.Meetings.GetByIdAsync(id);
            if (meeting == null)
            {
                return NotFound(new { success = false, message = "Meeting not found" });
            }

            meeting.ValidationStatus = request.Status;
            meeting.ValidatorId = GetCurrentUserId();
            await _unitOfWork.Meetings.UpdateAsync(meeting);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { success = true, message = "Meeting status updated", data = MapMeeting(meeting) });
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMeetingById(Guid id)
        {
            var meeting = await _unitOfWork.Meetings.GetByIdAsync(id);
            if (meeting == null)
            {
                return NotFound(new { success = false, message = "Meeting not found" });
            }

            return Ok(new { success = true, message = "Meeting fetched successfully", data = MapMeeting(meeting) });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetMeetings([FromQuery] Guid? projectId)
        {
            var meetings = projectId.HasValue
                ? await _unitOfWork.Meetings.GetAsync(m => m.ProjectId == projectId.Value)
                : await _unitOfWork.Meetings.GetAllAsync();

            return Ok(new { success = true, message = "Meetings fetched successfully", data = meetings.OrderByDescending(m => m.ScheduledDate) });
        }

        private static object MapMeeting(PfeManagement.Domain.Entities.Meeting meeting)
        {
            return new
            {
                id = meeting.Id,
                scheduledDate = meeting.ScheduledDate,
                agenda = meeting.Agenda,
                actualMinutes = meeting.ActualMinutes,
                referenceType = meeting.ReferenceType,
                referenceId = meeting.ReferenceId,
                createdById = meeting.CreatedById,
                validationStatus = meeting.ValidationStatus,
                validatorId = meeting.ValidatorId,
                projectId = meeting.ProjectId,
                createdAt = meeting.CreatedAt,
                updatedAt = meeting.UpdatedAt
            };
        }

        private Guid? GetCurrentUserId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(sub, out var userId) ? userId : null;
        }

        public class UpdateMeetingStatusRequest
        {
            public ValidationStatus Status { get; set; }
        }
    }
}
