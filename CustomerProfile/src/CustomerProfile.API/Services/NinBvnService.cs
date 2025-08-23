using CustomerAPI.Data;
using CustomerAPI.DTO;
using CustomerAPI.DTO.BvnNinVerification;
using CustomerAPI.External;

namespace CustomerAPI.Services
{
    public class NinBvnService(
        UserProfileDbContext _context,
        QuickVerifyBvnNinService quickVerifyBvnNinService,
        FaceRecognitionService faceRecognitionService)
    {
        public async Task<ApiResponse<bool>> SearchBvnAsync(Guid validUserId, BvnSearchRequest request)
        {
            var validator = new BvnSearchRequestValidator();
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return ApiResponse<bool>
                    .Error(validationResult.Errors.Select(s =>
                    new { s.ErrorMessage, s.AttemptedValue }));
            }
            var user = await _context.UserProfiles.FindAsync(validUserId);
            if (user is null)
                return ApiResponse<bool>.Error("User not found, try login again");

            BvnApiResponse? bvnSearchResult = await quickVerifyBvnNinService.BvnSearchRequest(request.Bvn);
            if (bvnSearchResult is null)
                return ApiResponse<bool>.Error("BVN service is currently unavailable, try again later");

            if (bvnSearchResult.Status == false)
                return ApiResponse<bool>.Error(bvnSearchResult.Detail ?? "BVN not found");

            user.AddBvn(bvnSearchResult);
            await _context.SaveChangesAsync();

            return ApiResponse<bool>.Success(true);
        }

        public async Task<ApiResponse<FaceComparisonResponse>> FaceVerificationAsync(Guid validUserId, FaceVerificationRequest request)
        {
            var validator = new FaceVerificationRequestValidator();
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return ApiResponse<FaceComparisonResponse>
                    .Error(validationResult.Errors.Select(s =>
                    new { s.ErrorMessage, s.AttemptedValue }));
            }
            var user = await _context.UserProfiles.FindAsync(validUserId);
            if (user is null)
                return ApiResponse<FaceComparisonResponse>.Error("User not found, try login again");

            if (user.BvnExists == false)
                return ApiResponse<FaceComparisonResponse>.Error("BVN not found, complete BVN search first");
            if (string.IsNullOrWhiteSpace(user.BvnBase64Image))
                return ApiResponse<FaceComparisonResponse>.Error("BVN image not found, complete BVN search first");

            return await faceRecognitionService
                    .CompareFaces(request.ImageFile, user.BvnBase64Image);
        }
    }
}
