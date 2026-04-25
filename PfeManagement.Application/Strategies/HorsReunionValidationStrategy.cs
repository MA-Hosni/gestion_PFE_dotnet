using System;
using System.Threading.Tasks;
using PfeManagement.Application.DTOs.Validations;
using PfeManagement.Domain.Entities;
using PfeManagement.Domain.Enums;

namespace PfeManagement.Application.Strategies
{
    // GoF Pattern: Strategy - Concrete Strategy
    public class HorsReunionValidationStrategy : IValidationStrategy
    {
        public Task<ValidationRecord> ValidateAsync(CreateValidationDto dto, Guid validatorId)
        {
            // No meeting reference needed for out-of-meeting validation
            var record = new ValidationRecord
            {
                TaskId = dto.TaskId,
                TaskStatus = dto.TaskStatus,
                Status = ValidationStatus.Valid, 
                ValidatorId = validatorId,
                MeetingType = MeetingType.HorsReunion,
                MeetingReferenceId = null,
                Comment = dto.Comment
            };

            return Task.FromResult(record);
        }
    }
}
