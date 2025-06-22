using System;
using OrderService.Domain.Interfaces;

namespace OrderService.Domain.Events
{
    public class OrderStatusUpdatedEvent : IDomainEvent
    {
        public Guid OrderId { get; }
        public string Status { get; }

        public OrderStatusUpdatedEvent(Guid orderId, string status)
        {
            OrderId = orderId;
            Status = status;
        }
    }
} 