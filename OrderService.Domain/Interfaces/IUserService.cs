using System;
using System.Threading.Tasks;

namespace OrderService.Domain.Interfaces
{
    public interface IUserService
    {
        Task<bool> ValidateUserAsync(Guid userId);
    }
} 