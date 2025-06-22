using OrderService.Domain.Interfaces;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Services
{
    public class InventoryServiceClient : IInventoryService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public InventoryServiceClient(HttpClient httpClient, string baseUrl)
        {
            _httpClient = httpClient;
            _baseUrl = baseUrl;
        }

        public async Task<bool> CheckAvailabilityAsync(Guid productId, int quantity)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/inventory/check?productId={productId}&quantity={quantity}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                // В случае ошибки считаем, что товар недоступен
                return false;
            }
        }

        public async Task UpdateStockAsync(Guid productId, int quantity)
        {
            var request = new
            {
                ProductId = productId,
                Quantity = quantity
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/inventory/update", content);
            response.EnsureSuccessStatusCode();
        }
    }
}
