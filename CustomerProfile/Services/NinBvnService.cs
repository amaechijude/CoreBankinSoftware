using CustomerProfile.Data;
using CustomerProfile.DTO;
using CustomerProfile.DTO.BvnNinVerification;
using CustomerProfile.Entities;
using CustomerProfile.External;
using CustomerProfile.JwtTokenService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CustomerProfile.Services;

public sealed class NinBvnService(
    UserProfileDbContext _context,
    QuickVerifyBvnNinService quickVerifyBvnNinService,
    FaceRecognitionService faceRecognitionService,
    JwtTokenProviderService jwtTokenProvider
)
{
    private readonly PasswordHasher<UserProfile> _passwordHasher = new();

    public async Task<ApiResponse<bool>> SearchBvnAsync(
        Guid validUserId,
        BvnSearchRequest request,
        CancellationToken ct
    )
    {
        var validator = new BvnSearchRequestValidator();
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return ApiResponse<bool>.Error(
                validationResult
                    .Errors.Select(s => new { s.ErrorMessage, s.AttemptedValue })
                    .ToList()
            );
        }
        var user = await _context.UserProfiles.FindAsync([validUserId], cancellationToken: ct);
        if (user is null)
            return ApiResponse<bool>.Error("User not found, try login again");

        BvnApiResponse? bvnSearchResult = await quickVerifyBvnNinService.BvnSearchRequest(
            request.Bvn
        );
        if (bvnSearchResult is null)
            return ApiResponse<bool>.Error("BVN service is currently unavailable, try again later");

        if (bvnSearchResult.Status == false)
            return ApiResponse<bool>.Error(bvnSearchResult.Detail ?? "BVN not found");

        user.AddBvn(bvnSearchResult);
        await _context.SaveChangesAsync(ct);

        return ApiResponse<bool>.Success(true);
    }

    public async Task<ApiResponse<string>> FaceVerificationAsync(
        Guid validUserId,
        FaceVerificationRequest request,
        CancellationToken ct
    )
    {
        var validator = new FaceVerificationRequestValidator();
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return ApiResponse<string>.Error(
                validationResult.Errors.Select(s => new { s.ErrorMessage, s.AttemptedValue })
            );
        }
        var user = await _context.UserProfiles.FindAsync([validUserId], cancellationToken: ct);
        if (user is null)
            return ApiResponse<string>.Error("User not found, try login again");

        if (user.BvnExists == false)
            return ApiResponse<string>.Error("BVN not found, complete BVN search first");
        if (string.IsNullOrWhiteSpace(user.BvnBase64Image))
            return ApiResponse<string>.Error("BVN image not found, complete BVN search first");

        var result = await faceRecognitionService.CompareFaces(
            request.ImageFile,
            user.BvnBase64Image
        );

        if (!result.IsSimilar)
            return ApiResponse<string>.Error(
                "Face verification failed; Try again in a better light condition"
            );

        await _context.SaveChangesAsync(ct);
        return ApiResponse<string>.Success("Face verification successful");
    }

    public async Task<ApiResponse<UserProfileResponse>> HandleSetProfileAsync(
        Guid validId,
        SetProfileRequest request,
        CancellationToken ct
    )
    {
        var validator = new SetDetailsRequestValidator();
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return ApiResponse<UserProfileResponse>.Error(
                validationResult.Errors.Select(s => new { s.ErrorMessage, s.AttemptedValue })
            );
        }

        var vCode = await _context.VerificationCodes.FindAsync([validId], cancellationToken: ct);
        if (vCode is null || !vCode.CanSetProfile)
            return ApiResponse<UserProfileResponse>.Error("Request Timeout");

        if (
            await _context.UserProfiles.AnyAsync(
                u => u.PhoneNumber == vCode.UserPhoneNumber,
                cancellationToken: ct
            )
        )
            return ApiResponse<UserProfileResponse>.Error("Possible duplicate request, Try Login");

        var user = UserProfile.CreateNewUser(
            vCode.UserPhoneNumber,
            email: vCode.UserEmail,
            request.Username
        );
        var passwordHash = _passwordHasher.HashPassword(user, request.Password);
        user.AddPasswordHash(passwordHash);

        _context.UserProfiles.Add(user);

        // delete vCode with executedeleteasync
        await _context
            .VerificationCodes.Where(v => v.Id == vCode.Id)
            .ExecuteDeleteAsync(cancellationToken: ct);

        await _context.SaveChangesAsync(ct);

        return ApiResponse<UserProfileResponse>.Success(
            GenerateJWtAndMapToUserProfileResponse(user)
        );
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
            jwt
        );
    }
}
