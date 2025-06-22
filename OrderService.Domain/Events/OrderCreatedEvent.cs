using System;
using System.Collections.Generic;
using OrderService.Domain.Interfaces;

namespace OrderService.Domain.Events
{
    public class OrderCreatedEvent : IDomainEvent
    {
        public Guid OrderId { get; }
        public DateTime CreatedAt { get; }
        public IReadOnlyList<OrderItemDto> Items { get; }
        public decimal TotalPrice { get; }

        public OrderCreatedEvent(Guid orderId, IReadOnlyList<OrderItemDto> items, decimal totalPrice)
        {
            OrderId = orderId;
            CreatedAt = DateTime.UtcNow;
            Items = items;
            TotalPrice = totalPrice;
        }
    }
} 