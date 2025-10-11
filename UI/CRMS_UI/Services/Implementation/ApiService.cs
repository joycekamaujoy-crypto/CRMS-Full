using System.Text;
using System.Text.Json;

using CRMS_UI.Services.Interfaces;

namespace CRMS_UI.Services.Implementation
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["ApiSettings:BaseUrl"] ?? throw new InvalidOperationException("ApiSettings:BaseUrl is missing.");
        }

        private void AddAuthHeader(HttpRequestMessage request, HttpContext httpContext)
        {
            var token = httpContext.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }
        }

        public async Task<T> GetAsync<T>(string endpoint, HttpContext httpContext)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/{endpoint}");
            AddAuthHeader(request, httpContext);
            return await SendRequestAsync<T>(request);
        }

        public async Task<T> PostAsync<T, TData>(string endpoint, TData data, HttpContext httpContext)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/{endpoint}");
            request.Content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            AddAuthHeader(request, httpContext);
            return await SendRequestAsync<T>(request);
        }

        public async Task<T> PutAsync<T, TData>(string endpoint, TData data, HttpContext httpContext)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"{_baseUrl}/{endpoint}");
            request.Content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            AddAuthHeader(request, httpContext);
            return await SendRequestAsync<T>(request);
        }

        public async Task<bool> DeleteAsync(string endpoint, HttpContext httpContext)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{_baseUrl}/{endpoint}");
            AddAuthHeader(request, httpContext);
            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        private async Task<T> SendRequestAsync<T>(HttpRequestMessage request)
        {
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Error: {response.StatusCode}. Content: {errorContent}");
                throw new HttpRequestException($"API request failed with status code {response.StatusCode}.");
            }

            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(content) || content.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return (T)(object)true;
            }

            return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }
    }
}
