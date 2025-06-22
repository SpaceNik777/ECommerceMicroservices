using OrderService.Domain.Interfaces;
using OrderService.Domain.ValueObjects;
using OrderService.Domain.Entities;
using OrderService.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Domain.Aggregates
{
    // Domain/Aggregates/Order.cs
    public class Order : IAggregateRoot
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public OrderStatus Status { get; private set; }
        public Address ShippingAddress { get; private set; } = null!;
        public decimal TotalAmount { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

        private readonly List<OrderItem> _items = new();

        private readonly List<IDomainEvent> _domainEvents = new();
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        // Required for EF Core
        private Order() { }

        public Order(Guid userId, Address shippingAddress)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId is invalid");

            Id = Guid.NewGuid();
            UserId = userId;
            Status = OrderStatus.Created;
            ShippingAddress = shippingAddress ?? throw new ArgumentNullException(nameof(shippingAddress));
            CreatedAt = DateTime.UtcNow;
            TotalAmount = 0;

            AddDomainEvent(new OrderCreatedEvent(Id, _items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId.ToString(),
                Quantity = i.Quantity,
                Price = i.UnitPrice
            }).ToList(), TotalAmount));
        }

        public void PlaceOrder()
        {
            if (Status != OrderStatus.Created)
                throw new InvalidOperationException("Order can only be placed when in Created status");

            if (!_items.Any())
                throw new InvalidOperationException("Cannot place empty order");

            Status = OrderStatus.Placed;
            UpdatedAt = DateTime.UtcNow;
            _domainEvents.Add(new OrderPlacedEvent(Id, Items.ToList(), TotalAmount));
        }

        public void Cancel()
        {
            if (Status is OrderStatus.Shipped or OrderStatus.Cancelled)
                throw new InvalidOperationException("Order cannot be cancelled");

            Status = OrderStatus.Cancelled;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddItem(OrderItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            var existingItem = _items.FirstOrDefault(i => i.ProductId == item.ProductId);
            if (existingItem != null)
            {
                existingItem.UpdateQuantity(existingItem.Quantity + item.Quantity);
                RecalculateTotal();
                return;
            }

            _items.Add(item);
            item.SetOrder(this);
            RecalculateTotal();
        }

        public void Ship()
        {
            if (Status != OrderStatus.Placed)
                throw new InvalidOperationException("Order can only be shipped when in Placed status");

            Status = OrderStatus.Shipped;
            UpdatedAt = DateTime.UtcNow;
            _domainEvents.Add(new OrderShippedEvent(Id, UserId));
        }

        public void Deliver()
        {
            if (Status != OrderStatus.Shipped)
                throw new InvalidOperationException("Order can only be delivered when in Shipped status");

            Status = OrderStatus.Delivered;
            UpdatedAt = DateTime.UtcNow;
            _domainEvents.Add(new OrderDeliveredEvent(Id, UserId));
        }

        public void UpdateStatus(OrderStatus status)
        {
            Status = status;
            UpdatedAt = DateTime.UtcNow;
        }

        private void RecalculateTotal()
        {
            TotalAmount = _items.Sum(item => item.UnitPrice * item.Quantity);
        }

        public void ClearDomainEvents() => _domainEvents.Clear();
    }
}
