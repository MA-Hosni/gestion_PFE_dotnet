using System;
using System.Threading.Tasks;
using PfeManagement.Application.DTOs.Validations;
using PfeManagement.Domain.Entities;
using PfeManagement.Domain.Enums;
using PfeManagement.Domain.Interfaces;

namespace PfeManagement.Application.Strategies
{
    // GoF Pattern: Strategy - Concrete Strategy
    public class ReunionValidationStrategy : IValidationStrategy
    {
        private readonly IRepository<Meeting> _meetingRepo;

        public ReunionValidationStrategy(IRepository<Meeting> meetingRepo)
        {
            _meetingRepo = meetingRepo;
        }

        public async Task<ValidationRecord> ValidateAsync(CreateValidationDto dto, Guid validatorId)
        {
            if (dto.MeetingReferenceId == null)
                throw new InvalidOperationException("Reunion validation requires a meeting reference");

            var meeting = await _meetingRepo.GetByIdAsync(dto.MeetingReferenceId.Value);
            if (meeting == null)
            {
                throw new Exception("Meeting not found");
            }

            return new ValidationRecord
            {
                TaskId = dto.TaskId,
                TaskStatus = dto.TaskStatus,
                Status = ValidationStatus.Valid, // Logic can be more complex based on requirements
                ValidatorId = validatorId,
                MeetingType = MeetingType.Reunion,
                MeetingReferenceId = dto.MeetingReferenceId,
                Comment = dto.Comment
            };
        }
    }
}
