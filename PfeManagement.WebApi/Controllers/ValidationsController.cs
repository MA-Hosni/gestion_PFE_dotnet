using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PfeManagement.Application.DTOs.Validations;
using PfeManagement.Application.Interfaces;
using PfeManagement.Domain.Interfaces;

namespace PfeManagement.WebApi.Controllers
{
    [ApiController]
    [Route("api/validations")]
    public class ValidationsController : ControllerBase
    {
        private readonly IValidationService _validationService;
        private readonly IUnitOfWork _unitOfWork;

        public ValidationsController(IValidationService validationService, IUnitOfWork unitOfWork)
        {
            _validationService = validationService;
            _unitOfWork = unitOfWork;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateValidation([FromBody] CreateValidationDto dto)
        {
            try
            {
                var validatorId = GetCurrentUserId() ?? Guid.Empty;
                var result = await _validationService.CreateValidationAsync(dto, validatorId);
                return StatusCode(201, new { success = true, message = "Validation created successfully", data = MapValidation(result) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetValidations([FromQuery] Guid? taskId)
        {
            var validations = taskId.HasValue
                ? await _unitOfWork.Validations.GetAsync(v => v.TaskId == taskId.Value)
                : await _unitOfWork.Validations.GetAllAsync();

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
            var validation = await _unitOfWork.Validations.GetByIdAsync(id);
            if (validation == null)
            {
                return NotFound(new { success = false, message = "Validation not found" });
            }

            await _unitOfWork.Validations.DeleteAsync(validation);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { success = true, message = "Validation deleted successfully" });
        }

        private Guid? GetCurrentUserId()
        {
            var sub = User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type.EndsWith("nameidentifier", StringComparison.OrdinalIgnoreCase))?.Value;
            return Guid.TryParse(sub, out var userId) ? userId : null;
        }

        private static object MapValidation(PfeManagement.Domain.Entities.ValidationRecord validation)
        {
            return new
            {
                id = validation.Id,
                taskId = validation.TaskId,
                taskStatus = validation.TaskStatus,
                status = validation.Status,
                validatorId = validation.ValidatorId,
                meetingType = validation.MeetingType,
                meetingReferenceId = validation.MeetingReferenceId,
                comment = validation.Comment,
                createdAt = validation.CreatedAt,
                updatedAt = validation.UpdatedAt
            };
        }
    }
}
