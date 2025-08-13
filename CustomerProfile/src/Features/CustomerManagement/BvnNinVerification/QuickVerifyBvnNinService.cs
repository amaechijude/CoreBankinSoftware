using System.ComponentModel.DataAnnotations;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace src.Features.CustomerManagement.BvnNinVerification
{
    public static class QuickVerifyEndpoint
    {
        public static void MapEndpoints(this WebApplication app)
        {
            app.MapPost("nin-search", async (NinSearchRequest request, QuickVerifyBvnNinService quickVerifyBvnNinService) =>
            {
                var validator = new NinRequestValidator();
                var validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => new { e.ErrorMessage, e.AttemptedValue });
                    return Results.BadRequest(errors);
                }
                var result = await quickVerifyBvnNinService.NINSearchRequest(request.NIN.Trim());

            });
        }
    }
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

        public async Task<BvnApiResponse?> BvnSearchRequest(string bvnNumber)
        {
            string subUrl = "bvn-search";
            var body = new { bvn = bvnNumber };
            var json = System.Text.Json.JsonSerializer.Serialize(body);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(subUrl, content);
            if (!response.IsSuccessStatusCode)
                return null;

            var responseContent = await response.Content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<BvnApiResponse>(responseContent);

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
    
    

    public record NinSearchRequest(string NIN);
    public class NinRequestValidator : AbstractValidator<NinSearchRequest>
    {
        public NinRequestValidator()
        {
            RuleFor(x => x.NIN.Trim())
                .NotEmpty().WithMessage("NIN is required.")
                .Length(11).WithMessage("NIN lenght Must be 11")
                .Matches(@"^\d{11}$").WithMessage("Invalid NIN format: Nin is not 11 digits");
        }
    }
        
}
