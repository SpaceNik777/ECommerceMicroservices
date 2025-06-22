using OrderService.Domain.Interfaces;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Services
{
    public class UserServiceClient : IUserService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public UserServiceClient(HttpClient httpClient, string baseUrl)
        {
            _httpClient = httpClient;
            _baseUrl = baseUrl;
        }

        public async Task<bool> ValidateUserAsync(Guid userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/users/validate?id={userId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                // В случае ошибки считаем, что пользователь не существует
                return false;
            }
        }
    }
}
