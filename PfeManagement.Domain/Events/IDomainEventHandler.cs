using System;
using System.Threading.Tasks;

namespace PfeManagement.Domain.Events
{
    public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
    {
        Task HandleAsync(TEvent domainEvent);
    }
}
