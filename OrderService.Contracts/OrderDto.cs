using System;
using System.Collections.Generic;

namespace OrderService.Contracts
{
    public record OrderDto(
        Guid Id,
        Guid UserId,
        string Status,
        AddressDto ShippingAddress,
        IReadOnlyList<OrderItemDto> Items,
        decimal Total);

    public record AddressDto(
        string Street,
        string City,
        string State,
        string Country,
        string ZipCode);

    public record OrderItemDto(
        Guid ProductId,
        int Quantity,
        decimal UnitPrice);
} 