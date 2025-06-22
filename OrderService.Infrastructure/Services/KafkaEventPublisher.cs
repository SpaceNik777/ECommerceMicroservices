using System;
using System.Text.Json;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderService.Domain.Interfaces;
using OrderService.Domain.Events;

namespace OrderService.Infrastructure.Services
{
    public class KafkaEventPublisher : IEventPublisher
    {
        private readonly IProducer<string, string> _producer;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ILogger<KafkaEventPublisher> _logger;

        public KafkaEventPublisher(IConfiguration configuration, ILogger<KafkaEventPublisher> logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            var bootstrapServers = configuration["Kafka:BootstrapServers"];
            _logger.LogInformation("Инициализация Kafka producer с bootstrap servers: {BootstrapServers}", bootstrapServers);
            
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = bootstrapServers
            };
            _producer = new ProducerBuilder<string, string>(producerConfig).Build();
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            
            _logger.LogInformation("Kafka producer успешно инициализирован");
        }

        public async Task PublishAsync(string topic, IDomainEvent @event)
        {
            try
            {
                _logger.LogInformation("Публикация события типа {EventType} в топик {Topic}", @event.GetType().Name, topic);
                
                string serializedEvent;
                if (@event is OrderPlacedEvent orderPlacedEvent)
                {
                    serializedEvent = JsonSerializer.Serialize(orderPlacedEvent, _jsonOptions);
                }
                else
                {
                    serializedEvent = JsonSerializer.Serialize(@event, @event.GetType(), _jsonOptions);
                }
                
                _logger.LogInformation("Событие успешно сериализовано: {SerializedEvent}", serializedEvent);
                
                var message = new Message<string, string>
                {
                    Key = @event.GetType().Name,
                    Value = serializedEvent
                };

                var result = await _producer.ProduceAsync(topic, message);
                _logger.LogInformation("Событие успешно опубликовано в топик {Topic}, партиция {Partition}, смещение {Offset}", 
                    result.Topic, result.Partition, result.Offset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при публикации события в топик {Topic}", topic);
                throw new Exception($"Ошибка при публикации события в топик {topic}", ex);
            }
        }

        public void Dispose()
        {
            _logger.LogInformation("Освобождение ресурсов Kafka producer");
            _producer?.Dispose();
        }
    }
} 