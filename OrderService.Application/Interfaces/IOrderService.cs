using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrderService.Domain.DTOs;

namespace OrderService.Application.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(Guid userId, AddressDto address, IReadOnlyList<OrderItemDto> items);
        Task<OrderDto> GetOrderAsync(Guid orderId);
        Task CancelOrderAsync(Guid orderId);
        Task UpdateOrderStatusAsync(Guid orderId, string status);
    }
}
