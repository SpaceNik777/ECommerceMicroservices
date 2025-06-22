using System;
using System.Threading.Tasks;

namespace OrderService.Domain.Interfaces
{
    public interface IInventoryService
    {
        Task<bool> CheckAvailabilityAsync(Guid productId, int quantity);
        Task UpdateStockAsync(Guid productId, int quantity);
    }
} 