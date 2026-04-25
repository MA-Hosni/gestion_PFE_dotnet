using System;
using System.Threading.Tasks;
using PfeManagement.Domain.Events;

namespace PfeManagement.Application.Interfaces
{
    // Event Dispatcher Interface
    public interface IDomainEventDispatcher
    {
        Task DispatchAsync<TEvent>(TEvent domainEvent) where TEvent : IDomainEvent;
    }
}
