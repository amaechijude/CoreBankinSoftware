using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace src.Features.Customer.BvnNINVerification
{
    public sealed class QuickVerifyBvnNinService(QuickVerifyHttpClient client)
    {
        private readonly HttpClient _client = client.Client;

        // Post Method to verify NIN
        public async Task<NINAPIResponse?> NINSearchRequest(string ninNumber)
        {
            string subUrl = "nin-search";
            var body = new { nin = ninNumber };
            var json = System.Text.Json.JsonSerializer.Serialize(body);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(subUrl, content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return System.Text.Json.JsonSerializer.Deserialize<NINAPIResponse>(responseContent);
            }
            return null;

        }

    }

    // Quick verify secretes
    public sealed class QuickVerifySettings
    {
        [Required, Url, MinLength(10)]
        public string BaseUrl { get; set; } = string.Empty;
        [Required, MinLength(10)]
        public string ApiKey { get; set; } = string.Empty;
        [Required, MinLength(5)]
        public string AuthPrefix { get; set; } = string.Empty;
    }

    // QuickVerifyHttpClient
    public sealed class QuickVerifyHttpClient
    {
        private readonly QuickVerifySettings _settings;
        public HttpClient Client { get; private set; }

        public QuickVerifyHttpClient(IOptions<QuickVerifySettings> settings, HttpClient httpClient)
        {
            _settings = settings.Value;

            httpClient.BaseAddress = new Uri(_settings.BaseUrl);
            httpClient.DefaultRequestHeaders.Add(_settings.AuthPrefix, _settings.ApiKey);
            Client = httpClient;
        }
        
    }
        
}
