using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderService.Domain.Interfaces;
using OrderService.Domain.Entities;

namespace OrderService.Domain.Events
{
    // Domain/Events/OrderPlacedEvent.cs
    public class OrderPlacedEvent : IDomainEvent
    {
        [JsonPropertyName("orderId")]
        public string OrderId { get; }

        [JsonPropertyName("items")]
        public List<OrderItemDto> Items { get; }

        [JsonPropertyName("totalPrice")]
        public decimal TotalPrice { get; }

        public OrderPlacedEvent(Guid orderId, List<OrderItem> items, decimal totalPrice)
        {
            OrderId = orderId.ToString();
            Items = items.Select(i => new OrderItemDto 
            { 
                ProductId = i.ProductId.ToString(),
                Quantity = i.Quantity,
                Price = i.UnitPrice
            }).ToList();
            TotalPrice = totalPrice;
        }
    }

    public class OrderItemDto
    {
        [JsonPropertyName("productId")]
        public string ProductId { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }
    }
}
