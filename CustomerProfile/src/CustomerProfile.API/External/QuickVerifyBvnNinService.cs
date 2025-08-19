using System.ComponentModel.DataAnnotations;
using CustomerAPI.DTO.BvnNinVerification;
using FluentValidation;

namespace CustomerAPI.External
{
    public sealed class QuickVerifyBvnNinService(HttpClient client)
    {
        private readonly HttpClient _client = client;
        public async Task<NINAPIResponse?> NINSearchRequest(NinSearchRequest request)
        {
            var body = new { nin = request.NIN.Trim() };
            try
            {
                using HttpResponseMessage response = await _client.PostAsJsonAsync("nin-search", body);
                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<NINAPIResponse>();
            }
            catch {return null;}
        }

        public async Task<BvnApiResponse?> BvnSearchRequest(BvnSearchRequest request)
        {
            var body = new { bvn = request.BVN.Trim() };
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

    public record NinSearchRequest(string NIN);
    public class NinRequestValidator : AbstractValidator<NinSearchRequest>
    {
        public NinRequestValidator()
        {
            RuleFor(x => x.NIN.Trim())
                .NotEmpty().WithMessage("NIN is required.")
                .Matches(@"^\d{11}$").WithMessage("Invalid NIN format: Nin is not 11 digits");
        }
    }

    public record BvnSearchRequest(string BVN);
    public class BvnRequestValidator : AbstractValidator<BvnSearchRequest>
    {
        public BvnRequestValidator()
        {
            RuleFor(x => x.BVN.Trim())
                .NotEmpty().WithMessage("NIN is required.")
                .Matches(@"^\d{11}$").WithMessage("Invalid NIN format: Nin is not 11 digits");
        }
    }

}
