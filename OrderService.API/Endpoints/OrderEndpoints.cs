using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderService.Domain.DTOs;
using OrderService.Domain.Aggregates;
using OrderService.Domain.Interfaces;
using OrderService.Domain.ValueObjects;

namespace OrderService.API.Endpoints
{
    // API/Endpoints/OrderEndpoints.cs
    public static class OrderEndpoints
    {
        public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/orders")
                .WithTags("Orders");

            group.MapPost("/", async (
                [FromBody] CreateOrderRequest request,
                [FromServices] IOrderService orderService) =>
            {
                var order = await orderService.CreateOrderAsync(
                    request.UserId,
                    request.ShippingAddress,
                    request.Items);

                return Results.Created($"/api/orders/{order.Id}", order);
            })
            .WithName("CreateOrder")
            .WithOpenApi();

            group.MapGet("/{id}", async (
                Guid id,
                [FromServices] IOrderService orderService) =>
            {
                var order = await orderService.GetOrderAsync(id);
                return order == null ? Results.NotFound() : Results.Ok(order);
            })
            .WithName("GetOrder")
            .WithOpenApi();

            group.MapPut("/{id}/cancel", async (
                Guid id,
                [FromServices] IOrderService orderService) =>
            {
                await orderService.CancelOrderAsync(id);
                return Results.NoContent();
            })
            .WithName("CancelOrder")
            .WithOpenApi();

            group.MapPut("/{id}/status", async (
                Guid id,
                [FromBody] UpdateOrderStatusRequest request,
                [FromServices] IOrderService orderService) =>
            {
                await orderService.UpdateOrderStatusAsync(id, request.Status);
                return Results.NoContent();
            })
            .WithName("UpdateOrderStatus")
            .WithOpenApi();
        }
    }

    public record CreateOrderRequest(
        Guid UserId,
        AddressDto ShippingAddress,
        IReadOnlyList<OrderItemDto> Items);

    public record UpdateOrderStatusRequest(string Status);
}
