using FluentValidation;
using PfeManagement.Domain.Entities;

namespace PfeManagement.Application.Validators
{
    // OCL Constraint: context Sprint inv: self.endDate > self.startDate
    public class SprintValidator : AbstractValidator<Sprint>
    {
        public SprintValidator()
        {
            RuleFor(s => s.EndDate)
                .GreaterThan(s => s.StartDate)
                .WithMessage("End date must be after start date");

            RuleFor(s => s.Title)
                .NotEmpty()
                .WithMessage("Sprint title is required");
        }
    }
}
