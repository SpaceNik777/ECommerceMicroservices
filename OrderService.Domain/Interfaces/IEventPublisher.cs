using System.Threading.Tasks;

namespace OrderService.Domain.Interfaces
{
    public interface IEventPublisher
    {
        Task PublishAsync(string topic, IDomainEvent @event);
    }
} 