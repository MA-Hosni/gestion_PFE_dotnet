using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PfeManagement.WebApi.Data;
using PfeManagement.WebApi.Models;

namespace PfeManagement.WebApi.Controllers
{
    [ApiController]
    [Route("api/validations")]
    public class ValidationsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ValidationsController(AppDbContext db)
        {
            _db = db;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateValidation([FromBody] CreateValidationDto dto)
        {
            try
            {
                var validatorId = GetCurrentUserId() ?? Guid.Empty;

                // Inline validation logic (was Strategy pattern before)
                ValidationRecord record;
                if (dto.MeetingType == MeetingType.Reunion)
                {
                    // Reunion validation - requires meeting reference
                    if (dto.MeetingReferenceId == null)
                        throw new Exception("Reunion validation requires a meeting reference");

                    var meeting = await _db.Meetings.FindAsync(dto.MeetingReferenceId.Value);
                    if (meeting == null)
                        throw new Exception("Meeting not found");

                    record = new ValidationRecord
                    {
                        TaskId = dto.TaskId,
                        TaskStatus = dto.TaskStatus,
                        Status = ValidationStatus.Valid,
                        ValidatorId = validatorId,
                        MeetingType = MeetingType.Reunion,
                        MeetingReferenceId = dto.MeetingReferenceId,
                        Comment = dto.Comment
                    };
                }
                else
                {
                    // HorsReunion validation - no meeting reference needed
                    record = new ValidationRecord
                    {
                        TaskId = dto.TaskId,
                        TaskStatus = dto.TaskStatus,
                        Status = ValidationStatus.Valid,
                        ValidatorId = validatorId,
                        MeetingType = MeetingType.HorsReunion,
                        MeetingReferenceId = null,
                        Comment = dto.Comment
                    };
                }

                // Update task status
                var task = await _db.Tasks.FindAsync(dto.TaskId);
                if (task == null) throw new Exception("Task not found");
                task.Status = dto.TaskStatus;

                _db.Validations.Add(record);
                await _db.SaveChangesAsync();

                return StatusCode(201, new { success = true, message = "Validation created successfully", data = MapValidation(record) });
            }
            catch (Exception ex) { return BadRequest(new { success = false, message = ex.Message }); }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetValidations([FromQuery] Guid? taskId)
        {
            var validations = taskId.HasValue
                ? await _db.Validations.Where(v => v.TaskId == taskId.Value).ToListAsync()
                : await _db.Validations.ToListAsync();

            return Ok(new
            {
                success = true,
                message = "Validations fetched successfully",
                data = validations.OrderByDescending(v => v.CreatedAt).Select(MapValidation)
            });
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteValidation(Guid id)
        {
            var validation = await _db.Validations.FindAsync(id);
            if (validation == null) return NotFound(new { success = false, message = "Validation not found" });
            _db.Validations.Remove(validation);
            await _db.SaveChangesAsync();
            return Ok(new { success = true, message = "Validation deleted successfully" });
        }

        private Guid? GetCurrentUserId()
        {
            var sub = User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type.EndsWith("nameidentifier", StringComparison.OrdinalIgnoreCase))?.Value;
            return Guid.TryParse(sub, out var id) ? id : null;
        }

        private static object MapValidation(ValidationRecord v) => new
        {
            id = v.Id, taskId = v.TaskId, taskStatus = v.TaskStatus, status = v.Status,
            validatorId = v.ValidatorId, meetingType = v.MeetingType,
            meetingReferenceId = v.MeetingReferenceId, comment = v.Comment,
            createdAt = v.CreatedAt, updatedAt = v.UpdatedAt
        };
    }
}
