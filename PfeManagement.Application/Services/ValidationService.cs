using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PfeManagement.Application.DTOs.Validations;
using PfeManagement.Application.Interfaces;
using PfeManagement.Application.Strategies;
using PfeManagement.Domain.Entities;
using PfeManagement.Domain.Enums;
using PfeManagement.Domain.Interfaces;

namespace PfeManagement.Application.Services
{
    public class ValidationService : IValidationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEnumerable<IValidationStrategy> _validationStrategies;

        public ValidationService(IUnitOfWork unitOfWork, IEnumerable<IValidationStrategy> validationStrategies)
        {
            _unitOfWork = unitOfWork;
            _validationStrategies = validationStrategies;
        }

        public async Task<ValidationRecord> CreateValidationAsync(CreateValidationDto dto, Guid validatorId)
        {
            // Simple strategy factory logic here (or injected via a factory)
            IValidationStrategy strategy = dto.MeetingType switch
            {
                MeetingType.Reunion => GetStrategy<ReunionValidationStrategy>(),
                MeetingType.HorsReunion => GetStrategy<HorsReunionValidationStrategy>(),
                _ => throw new Exception("Unknown validation context")
            };

            var record = await strategy.ValidateAsync(dto, validatorId);

            // Fetch the task and update status
            var task = await _unitOfWork.Tasks.GetByIdAsync(dto.TaskId);
            if (task == null) throw new Exception("Task not found");

            task.MoveTo(dto.TaskStatus);
            await _unitOfWork.Tasks.UpdateAsync(task);

            await _unitOfWork.Validations.AddAsync(record);
            await _unitOfWork.SaveChangesAsync();

            return record;
        }

        private IValidationStrategy GetStrategy<T>() where T : IValidationStrategy
        {
            foreach (var strategy in _validationStrategies)
            {
                if (strategy is T) return strategy;
            }
            throw new Exception($"Strategy {typeof(T).Name} not registered.");
        }
    }
}
