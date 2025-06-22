using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using OrderService.Domain.DTOs;

namespace OrderService.Domain.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(Guid userId, AddressDto shippingAddress, IReadOnlyList<OrderItemDto> items);
        Task<OrderDto> GetOrderAsync(Guid orderId);
        Task CancelOrderAsync(Guid orderId);
        Task UpdateOrderStatusAsync(Guid orderId, string status);
    }
} 