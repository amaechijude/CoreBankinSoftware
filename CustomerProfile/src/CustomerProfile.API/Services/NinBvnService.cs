using CustomerAPI.Data;
using CustomerAPI.DTO;
using CustomerAPI.DTO.BvnNinVerification;
using CustomerAPI.Entities;
using CustomerAPI.External;
using CustomerAPI.JwtTokenService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CustomerAPI.Services;

public class NinBvnService(
    UserProfileDbContext _context,
    QuickVerifyBvnNinService quickVerifyBvnNinService,
    FaceRecognitionService faceRecognitionService,
    JwtTokenProviderService jwtTokenProvider)
{
    private readonly PasswordHasher<UserProfile> _passwordHasher = new();
    public async Task<ApiResponse<bool>> SearchBvnAsync(Guid validUserId, BvnSearchRequest request)
    {
        var validator = new BvnSearchRequestValidator();
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return ApiResponse<bool>
                .Error(validationResult.Errors.Select(s =>
                new { s.ErrorMessage, s.AttemptedValue }).ToList());
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

    public async Task<ApiResponse<string>> FaceVerificationAsync(Guid validUserId, FaceVerificationRequest request)
    {
        var validator = new FaceVerificationRequestValidator();
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return ApiResponse<string>
                .Error(validationResult.Errors.Select(s =>
                new { s.ErrorMessage, s.AttemptedValue }));
        }
        var user = await _context.UserProfiles.FindAsync(validUserId);
        if (user is null)
            return ApiResponse<string>.Error("User not found, try login again");

        if (user.BvnExists == false)
            return ApiResponse<string>.Error("BVN not found, complete BVN search first");
        if (string.IsNullOrWhiteSpace(user.BvnBase64Image))
            return ApiResponse<string>.Error("BVN image not found, complete BVN search first");

        var result = await faceRecognitionService
                .CompareFaces(request.ImageFile, user.BvnBase64Image);

        if (!result.IsSimilar)
            return ApiResponse<string>.Error("Face verification failed; Try again in a better light condition");

        var account = Account.CreateNewAccount(user);
        await _context.Accounts.AddAsync(account);
        await _context.SaveChangesAsync();
        return ApiResponse<string>.Success("Face verification successful");

    }

    public async Task<ApiResponse<UserProfileResponse>> HandleSetProfileAsync(Guid validId, SetProfileRequest request)
    {
        var validator = new SetDetailsRequestValidator();
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return ApiResponse<UserProfileResponse>
                .Error(validationResult.Errors.Select(s =>
                new { s.ErrorMessage, s.AttemptedValue }));
        }

        var vCode = await _context.VerificationCodes.FindAsync(validId);
        if (vCode is null || !vCode.CanSetProfile)
            return ApiResponse<UserProfileResponse>.Error("Request Timeout");

        if (await _context.UserProfiles
            .AnyAsync(u => u.PhoneNumber == vCode.UserPhoneNumber))
            return ApiResponse<UserProfileResponse>.Error("Possible duplicate request, Try Login");

        var user = UserProfile
            .CreateNewUser(vCode.UserPhoneNumber, email: vCode.UserEmail, request.Username);

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _context.UserProfiles.Add(user);

        // delete vCode with executedeleteasync
        await _context.VerificationCodes
            .Where(v => v.Id == vCode.Id)
            .ExecuteDeleteAsync();

        await _context.SaveChangesAsync();

        return ApiResponse<UserProfileResponse>
            .Success(GenerateJWtAndMapToUserProfileResponse(user));
    }
    private UserProfileResponse GenerateJWtAndMapToUserProfileResponse(UserProfile user)
    {
        var (token, expiresIn) = jwtTokenProvider.GenerateUserJwtToken(user);

        var jwt = new Jwt(token, expiresIn);
        return new UserProfileResponse(
            user.Id,
            user.Username,
            user.Email,
            user.PhoneNumber,
            user.ImageUrl,
            jwt);
    }
}
