using System.Linq;
using FluentValidation;
using PfeManagement.Domain.Entities;

namespace PfeManagement.Application.Validators
{
    // OCL Constraint: context Project inv: self.sprints->forAll(s1, s2 | s1 <> s2 implies s1.orderIndex <> s2.orderIndex)
    public class ProjectValidator : AbstractValidator<Project>
    {
        public ProjectValidator()
        {
            RuleFor(p => p.Sprints)
                .Must(sprints => sprints.Select(s => s.OrderIndex).Distinct().Count() == sprints.Count)
                .When(p => p.Sprints != null && p.Sprints.Any())
                .WithMessage("Sprint order indices must be unique within a project");
        }
    }
}
