using System;
using System.Threading.Tasks;
using PfeManagement.Application.DTOs.Meetings;
using PfeManagement.Application.Interfaces;
using PfeManagement.Domain.Entities;
using PfeManagement.Domain.Interfaces;

namespace PfeManagement.Application.Services
{
    public class MeetingService : IMeetingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MeetingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Meeting> CreateMeetingAsync(CreateMeetingDto dto, Guid creatorId)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(dto.ProjectId);
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

            await _unitOfWork.Meetings.AddAsync(meeting);
            await _unitOfWork.SaveChangesAsync();

            return meeting; // Usually mapped, skipping for brevity
        }

        private static DateTime NormalizeUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };
        }
    }
}
