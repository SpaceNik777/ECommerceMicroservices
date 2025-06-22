using System;
using System.Collections.Generic;

namespace OrderService.Contracts
{
    public record CreateOrderRequest(
        Guid UserId,
        AddressDto ShippingAddress,
        IReadOnlyList<OrderItemDto> Items
    );
} 