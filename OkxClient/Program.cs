using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OkexApiTest
{
    public class OkexApiClient
    {
        private const string ApiKey = "?????";
        private const string SecretKey = "??????";
        private const string BaseUrl = "https://www.okex.com";

        private readonly HttpClient _httpClient;
        private readonly int _timestampPaddingSeconds = 10;

        private string _timestamp;

        public OkexApiClient()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl)
            };

            // Generate timestamp once during the constructor call
            _timestamp = DateTimeOffset.UtcNow.AddSeconds(_timestampPaddingSeconds).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        }

        private string GenerateSignature(string method, string requestPath, string body = "")
        {
            var prehashString = $"{_timestamp}{method.ToUpper()}{requestPath}{body}";
            var secretBytes = Encoding.UTF8.GetBytes(SecretKey);
            using (var hmac = new HMACSHA256(secretBytes))
            {
                var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(prehashString));
                return Convert.ToBase64String(signatureBytes);
            }
        }

        public HttpRequestMessage CreateRequest(string method, string requestPath, string body = "")
        {
            var signature = GenerateSignature(method, requestPath, body);

            var request = new HttpRequestMessage(new HttpMethod(method), requestPath);
            request.Headers.Add("OK-ACCESS-KEY", ApiKey);
            request.Headers.Add("OK-ACCESS-SIGN", signature);
            request.Headers.Add("OK-ACCESS-TIMESTAMP", _timestamp);
            request.Headers.Add("OK-ACCESS-PASSPHRASE", "?????"); // Replace with your passphrase
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            return request;
        }

        public async Task<string> SendRequest(HttpRequestMessage request)
        {
            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            return responseContent;
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var okexApiClient = new OkexApiClient();

            try
            {
                // Example: Test the '/api/v5/account/balance' endpoint
                var endpoint = "/api/v5/account/balance";
                var request = okexApiClient.CreateRequest("GET", endpoint);

                var response = await okexApiClient.SendRequest(request);
                Console.WriteLine("API Response:");
                Console.WriteLine(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
