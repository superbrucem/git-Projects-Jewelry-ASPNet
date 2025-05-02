using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;

namespace OttawaOpalShop.Services
{
    public class PayPalService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger<PayPalService> _logger;
        private string _accessToken = string.Empty;
        private DateTime _tokenExpiration = DateTime.MinValue;

        public PayPalService(IConfiguration configuration, HttpClient httpClient, ILogger<PayPalService> logger)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _logger = logger;
            
            // Set base address based on mode (sandbox or production)
            string mode = _configuration["PayPal:Mode"] ?? "sandbox";
            _httpClient.BaseAddress = new Uri(mode == "sandbox" 
                ? "https://api-m.sandbox.paypal.com/" 
                : "https://api-m.paypal.com/");
        }

        private async Task EnsureAccessTokenAsync()
        {
            // If token is still valid, return
            if (!string.IsNullOrEmpty(_accessToken) && _tokenExpiration > DateTime.UtcNow)
            {
                return;
            }

            try
            {
                // Get credentials from configuration
                string clientId = _configuration["PayPal:ClientId"] ?? throw new InvalidOperationException("PayPal ClientId is not configured");
                string secret = _configuration["PayPal:Secret"] ?? throw new InvalidOperationException("PayPal Secret is not configured");

                // Create authorization header
                var authToken = Encoding.UTF8.GetBytes($"{clientId}:{secret}");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));

                // Prepare request body
                var requestData = new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" }
                };

                // Send request to get access token
                var response = await _httpClient.PostAsync("v1/oauth2/token", new FormUrlEncodedContent(requestData));
                response.EnsureSuccessStatusCode();

                // Parse response
                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                _accessToken = tokenResponse.GetProperty("access_token").GetString() ?? string.Empty;
                int expiresIn = tokenResponse.GetProperty("expires_in").GetInt32();
                
                // Set token expiration (subtract 5 minutes for safety)
                _tokenExpiration = DateTime.UtcNow.AddSeconds(expiresIn).AddMinutes(-5);
                
                // Update authorization header for future requests
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                
                _logger.LogInformation("PayPal access token obtained successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to obtain PayPal access token");
                throw;
            }
        }

        public async Task<string> CreateOrderAsync(decimal amount, string currency = "USD", string description = "Ottawa Opal Shop Purchase")
        {
            await EnsureAccessTokenAsync();

            try
            {
                // Prepare order request
                var orderRequest = new
                {
                    intent = "CAPTURE",
                    purchase_units = new[]
                    {
                        new
                        {
                            amount = new
                            {
                                currency_code = currency,
                                value = amount.ToString("0.00")
                            },
                            description
                        }
                    },
                    application_context = new
                    {
                        brand_name = "Ottawa Opal Shop",
                        landing_page = "BILLING",
                        user_action = "PAY_NOW",
                        return_url = "https://localhost:7220/Cart/CapturePayment",
                        cancel_url = "https://localhost:7220/Cart/CancelPayment"
                    }
                };

                // Convert to JSON
                var jsonContent = JsonSerializer.Serialize(orderRequest);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Send request to create order
                var response = await _httpClient.PostAsync("v2/checkout/orders", content);
                response.EnsureSuccessStatusCode();

                // Parse response
                var responseContent = await response.Content.ReadAsStringAsync();
                var orderResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                string orderId = orderResponse.GetProperty("id").GetString() ?? string.Empty;
                _logger.LogInformation($"PayPal order created successfully: {orderId}");
                
                return orderId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create PayPal order");
                throw;
            }
        }

        public async Task<bool> CaptureOrderAsync(string orderId)
        {
            await EnsureAccessTokenAsync();

            try
            {
                // Send request to capture payment
                var response = await _httpClient.PostAsync($"v2/checkout/orders/{orderId}/capture", new StringContent("{}", Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();

                // Parse response
                var responseContent = await response.Content.ReadAsStringAsync();
                var captureResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                string status = captureResponse.GetProperty("status").GetString() ?? string.Empty;
                bool success = status.Equals("COMPLETED", StringComparison.OrdinalIgnoreCase);
                
                _logger.LogInformation($"PayPal order {orderId} capture status: {status}");
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to capture PayPal order {orderId}");
                throw;
            }
        }

        public async Task<string> GetApprovalUrlAsync(string orderId)
        {
            await EnsureAccessTokenAsync();

            try
            {
                // Get order details
                var response = await _httpClient.GetAsync($"v2/checkout/orders/{orderId}");
                response.EnsureSuccessStatusCode();

                // Parse response
                var responseContent = await response.Content.ReadAsStringAsync();
                var orderResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                // Find the approval URL
                var links = orderResponse.GetProperty("links").EnumerateArray();
                foreach (var link in links)
                {
                    string rel = link.GetProperty("rel").GetString() ?? string.Empty;
                    if (rel.Equals("approve", StringComparison.OrdinalIgnoreCase))
                    {
                        return link.GetProperty("href").GetString() ?? string.Empty;
                    }
                }
                
                throw new InvalidOperationException("Approval URL not found in PayPal order response");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get approval URL for PayPal order {orderId}");
                throw;
            }
        }
    }
}
