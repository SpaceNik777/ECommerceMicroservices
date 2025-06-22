using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.Domain.DTOs;
using OrderService.Domain.Aggregates;
using OrderService.Domain.Entities;
using OrderService.Domain.ValueObjects;
using OrderService.Domain.Interfaces;
using OrderService.Domain.Events;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Application.Services
{
    // Application/Services/OrderService.cs
    public class OrderService : Domain.Interfaces.IOrderService
    {
        private readonly OrderDbContext _context;
        private readonly IUserService _userService;
        private readonly IInventoryService _inventoryService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            OrderDbContext context,
            IUserService userService,
            IInventoryService inventoryService,
            IEventPublisher eventPublisher,
            ILogger<OrderService> logger)
        {
            _context = context;
            _userService = userService;
            _inventoryService = inventoryService;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        public async Task<OrderDto> CreateOrderAsync(Guid userId, AddressDto address, IReadOnlyList<Domain.DTOs.OrderItemDto> items)
        {
            _logger.LogInformation("Creating order for user {UserId}", userId);

            // Проверяем пользователя
            var isValidUser = await _userService.ValidateUserAsync(userId);
            if (!isValidUser)
            {
                _logger.LogWarning("User {UserId} not found", userId);
                throw new Exception($"User with ID {userId} not found");
            }

            _logger.LogInformation("User {UserId} validated successfully", userId);

            // Проверяем наличие товаров
            foreach (var item in items)
            {
                _logger.LogInformation("Checking availability for product {ProductId}, quantity: {Quantity}", 
                    item.ProductId, item.Quantity);

                var isAvailable = await _inventoryService.CheckAvailabilityAsync(item.ProductId, item.Quantity);
                if (!isAvailable)
                {
                    _logger.LogWarning("Insufficient stock for product {ProductId}", item.ProductId);
                    throw new Exception($"Insufficient stock for product {item.ProductId}");
                }
            }

            _logger.LogInformation("All products are available");

            // Создаем заказ
            var order = new Order(userId, new Address(
                address.Street,
                address.City,
                address.State,
                address.Country,
                address.ZipCode
            ));

            _logger.LogInformation("Order created with ID {OrderId}", order.Id);

            // Добавляем товары
            foreach (var item in items)
            {
                order.AddItem(new OrderItem(item.ProductId, item.Quantity, item.UnitPrice));
                _logger.LogInformation("Added item to order: ProductId={ProductId}, Quantity={Quantity}, UnitPrice={UnitPrice}",
                    item.ProductId, item.Quantity, item.UnitPrice);
            }

            // Размещаем заказ
            order.PlaceOrder();
            _logger.LogInformation("Order {OrderId} placed", order.Id);

            // Сохраняем заказ
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Order {OrderId} saved to database", order.Id);

            // Публикуем все доменные события
            foreach (var domainEvent in order.DomainEvents)
            {
                if (domainEvent is OrderPlacedEvent placedEvent)
                {
                    await _eventPublisher.PublishAsync("order-placed", placedEvent);
                    _logger.LogInformation("OrderPlacedEvent published for order {OrderId}", order.Id);
                }
            }

            return MapToDto(order);
        }

        public async Task<OrderDto> GetOrderAsync(Guid orderId)
        {
            _logger.LogInformation("Getting order {OrderId}", orderId);

            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found", orderId);
                throw new Exception($"Order with ID {orderId} not found");
            }

            _logger.LogInformation("Order {OrderId} retrieved successfully", orderId);
            return MapToDto(order);
        }

        public async Task CancelOrderAsync(Guid orderId)
        {
            _logger.LogInformation("Cancelling order {OrderId}", orderId);

            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found", orderId);
                throw new InvalidOperationException("Order not found");
            }

            order.Cancel();
            await _context.SaveChangesAsync();
            _logger.LogInformation("Order {OrderId} cancelled successfully", orderId);

            // Возвращаем товары на склад
            foreach (var item in order.Items)
            {
                await _inventoryService.UpdateStockAsync(item.ProductId, item.Quantity);
                _logger.LogInformation("Returned {Quantity} items of product {ProductId} to inventory",
                    item.Quantity, item.ProductId);
            }

            // Publish event
            await _eventPublisher.PublishAsync("order-status-updated", new OrderStatusUpdatedEvent(order.Id, order.Status.ToString()));
            _logger.LogInformation("OrderStatusUpdatedEvent published for order {OrderId}", orderId);
        }

        public async Task UpdateOrderStatusAsync(Guid orderId, string status)
        {
            _logger.LogInformation("Updating status of order {OrderId} to {Status}", orderId, status);

            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found", orderId);
                throw new InvalidOperationException("Order not found");
            }

            order.UpdateStatus((OrderStatus)Enum.Parse(typeof(OrderStatus), status));
            await _context.SaveChangesAsync();
            _logger.LogInformation("Order {OrderId} status updated to {Status}", orderId, status);

            // Publish event
            await _eventPublisher.PublishAsync("order-status-updated", new OrderStatusUpdatedEvent(order.Id, status));
            _logger.LogInformation("OrderStatusUpdatedEvent published for order {OrderId}", orderId);
        }

        private static OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                Status = order.Status.ToString(),
                ShippingAddress = new AddressDto
                {
                    Street = order.ShippingAddress.Street,
                    City = order.ShippingAddress.City,
                    State = order.ShippingAddress.State,
                    Country = order.ShippingAddress.Country,
                    ZipCode = order.ShippingAddress.ZipCode
                },
                Items = order.Items.Select(i => new Domain.DTOs.OrderItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList(),
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
            };
        }
    }
}
