using System;
using System.Collections.Generic;

namespace OrderService.Contracts
{
    public record OrderResponse(
        Guid Id,
        Guid UserId,
        string Status,
        AddressDto ShippingAddress,
        IReadOnlyList<OrderItemResponse> Items,
        decimal Total
    );

    public record OrderItemResponse(
        Guid ProductId,
        int Quantity,
        decimal UnitPrice
    );
} 