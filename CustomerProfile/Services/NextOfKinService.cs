using CustomerProfile.Data;
using CustomerProfile.DTO;
using CustomerProfile.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerProfile.Services;

public sealed class NextOfKinService(UserProfileDbContext context)
{
    private readonly UserProfileDbContext _context = context;

    public async Task<ApiResponse<string>> AddNextOfKinAsync(
        Guid userId,
        AddNextOfKinRequest request,
        CancellationToken ct
    )
    {
        var user = await _context.UserProfiles.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null)
        {
            return ApiResponse<string>.Error("User not found");
        }

        var nextOfKin = new NextOfKin
        {
            CustomerId = userId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName ?? string.Empty,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            Nationality = request.Nationality,
            Occupation = request.Occupation,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            AlternatePhoneNumber = request.AlternatePhoneNumber ?? string.Empty,
            Address = request.Address,
            Relationship = request.Relationship,
            Category = request.Category,
            CanContactInEmergency = request.CanContactInEmergency,
        };

        user.NextOfKins.Add(nextOfKin);
        await _context.SaveChangesAsync(ct);

        return ApiResponse<string>.Success("Next of Kin added successfully");
    }

    public async Task<ApiResponse<List<NextOfKinResponse>>> GetNextOfKinsAsync(
        Guid userId,
        CancellationToken ct
    )
    {
        var noks = await _context
            .UserProfiles.Where(u => u.Id == userId)
            .SelectMany(u => u.NextOfKins)
            .Select(n => new NextOfKinResponse(
                n.Id,
                n.FirstName,
                n.LastName,
                n.FullName,
                n.Relationship.ToString(),
                n.PhoneNumber,
                n.Email,
                n.Address
            ))
            .ToListAsync(ct);

        return ApiResponse<List<NextOfKinResponse>>.Success(noks);
    }

    public async Task<ApiResponse<string>> RemoveNextOfKinAsync(
        Guid userId,
        Guid nokId,
        CancellationToken ct
    )
    {
        var user = await _context
            .UserProfiles.Include(u => u.NextOfKins)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null)
        {
            return ApiResponse<string>.Error("User not found");
        }

        var nok = user.NextOfKins.FirstOrDefault(n => n.Id == nokId);
        if (nok is null)
        {
            return ApiResponse<string>.Error("Next of Kin not found");
        }

        user.NextOfKins.Remove(nok);
        await _context.SaveChangesAsync(ct);

        return ApiResponse<string>.Success("Next of Kin removed successfully");
    }
}
