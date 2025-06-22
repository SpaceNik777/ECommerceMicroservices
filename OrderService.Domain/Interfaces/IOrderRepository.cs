using System;
using System.Threading.Tasks;
using OrderService.Domain.Aggregates;

namespace OrderService.Domain.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order> GetByIdAsync(Guid id);
        Task AddAsync(Order order);
        Task UpdateAsync(Order order);
    }
} 