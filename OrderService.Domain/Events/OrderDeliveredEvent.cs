using System;
using OrderService.Domain.Interfaces;

namespace OrderService.Domain.Events
{
    public class OrderDeliveredEvent : IDomainEvent
    {
        public Guid OrderId { get; }
        public Guid UserId { get; }
        public DateTime OccurredOn { get; }

        public OrderDeliveredEvent(Guid orderId, Guid userId)
        {
            OrderId = orderId;
            UserId = userId;
            OccurredOn = DateTime.UtcNow;
        }
    }
} 