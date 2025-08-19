using CustomerAPI.DTO;
using CustomerAPI.DTO.BvnNinVerification;
using CustomerAPI.External;

namespace CustomerAPI.Services
{
    public class NinBvnService(QuickVerifyBvnNinService quickVerifyBvnNinService)
    {
        public async Task<ApiResponse<bool>> SearchNin(NinSearchRequest request)
        {
            var validator = new NinRequestValidator();
            var validate = await validator.ValidateAsync(request);
            if (!validate.IsValid)
            {
                var error = validate.Errors.Select(e => new { e.ErrorMessage, e.AttemptedValue });
                return ApiResponse<bool>.Error(error);
            }
            NINAPIResponse? nINAPIResponse = await quickVerifyBvnNinService.NINSearchRequest(request);
            if (nINAPIResponse is null)
            {
                // log error

                return ApiResponse<bool>.Error("Nin search failed");
            }
            return ApiResponse<bool>.Success(nINAPIResponse is null);
        }
    }
}
