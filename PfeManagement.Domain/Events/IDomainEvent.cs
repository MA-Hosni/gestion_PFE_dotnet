using System;

namespace PfeManagement.Domain.Events
{
    public interface IDomainEvent
    {
        DateTime OccurredAt { get; }
    }
}
