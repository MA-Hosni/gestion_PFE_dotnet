using FluentValidation;
using PfeManagement.Domain.Entities;
using PfeManagement.Domain.Enums;

namespace PfeManagement.Application.Validators
{
    // OCL Constraint: context Task inv: self.status = 'Done' implies self.assignedTo->notEmpty()
    public class TaskItemValidator : AbstractValidator<TaskItem>
    {
        public TaskItemValidator()
        {
            When(t => t.Status == TaskItemStatus.Done, () =>
            {
                RuleFor(t => t.AssignedToId)
                    .NotNull()
                    .WithMessage("Completed task must have an assignee");
            });

            RuleFor(t => t.Title)
                .NotEmpty()
                .WithMessage("Task title is required");
        }
    }
}
