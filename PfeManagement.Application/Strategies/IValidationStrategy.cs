using System;
using System.Threading.Tasks;
using PfeManagement.Application.DTOs.Validations;
using PfeManagement.Domain.Entities;

namespace PfeManagement.Application.Strategies
{
    // GoF Pattern: Strategy
    public interface IValidationStrategy
    {
        Task<ValidationRecord> ValidateAsync(CreateValidationDto dto, Guid validatorId);
    }
}
