using System;
using System.Collections.Generic;

namespace OrderService.Domain.DTOs
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Status { get; set; } = string.Empty;
        public AddressDto ShippingAddress { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }
} 