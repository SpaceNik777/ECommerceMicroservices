using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderService.Domain.Aggregates;

namespace OrderService.Domain.Entities
{
    public class OrderItem
    {
        public Guid Id { get; private set; }
        public Guid ProductId { get; private set; }
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public Guid OrderId { get; private set; }
        public Order Order { get; private set; } = null!;

        // Required for EF Core
        private OrderItem() { }

        public OrderItem(Guid productId, int quantity, decimal unitPrice)
        {
            if (productId == Guid.Empty)
                throw new ArgumentException("ProductId is invalid");

            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive");

            if (unitPrice <= 0)
                throw new ArgumentException("UnitPrice must be positive");

            Id = Guid.NewGuid();
            ProductId = productId;
            Quantity = quantity;
            UnitPrice = unitPrice;
        }

        public void UpdateQuantity(int newQuantity)
        {
            if (newQuantity <= 0)
                throw new ArgumentException("Invalid quantity");

            Quantity = newQuantity;
        }

        public void SetOrder(Order order)
        {
            Order = order;
            OrderId = order.Id;
        }
    }
}
