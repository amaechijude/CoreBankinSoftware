using System.ComponentModel.DataAnnotations;
using CustomerAPI.DTO.BvnNinVerification;

namespace CustomerAPI.External
{
    public sealed class QuickVerifyBvnNinService(HttpClient client)
    {
        private readonly HttpClient _client = client;
        public async Task<NINAPIResponse?> NINSearchRequest(string nin)
        {
            var body = new { nin = nin.Trim() };
            try
            {
                using HttpResponseMessage response = await _client.PostAsJsonAsync("nin-search", body);
                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<NINAPIResponse>();
            }
            catch {return null;}
        }

        public async Task<BvnApiResponse?> BvnSearchRequest(string bvn)
        {
            var body = new { bvn = bvn.Trim() };
            try
            {
                using HttpResponseMessage httpResponse = await _client
                    .PostAsJsonAsync("bvn-search", body);

                if (!httpResponse.IsSuccessStatusCode) return null;

                return await httpResponse.Content.ReadFromJsonAsync<BvnApiResponse>();
            }
            catch { return null; }
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
}
