using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace src.Infrastructure.External.BvnNINVerification
{
    public sealed class QuickVerifyBvnNinService(HttpClient client)
    {
        private readonly HttpClient _client = client;

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

    public sealed class QuickVerifySettings
    {
        [Required, Url, MinLength(10)]
        public string BaseUrl { get; set; } = string.Empty;
        [Required, MinLength(10)]
        public string ApiKey { get; set; } = string.Empty;
        [Required, MinLength(5)]
        public string AuthPrefix { get; set; } = string.Empty;
    }
}
