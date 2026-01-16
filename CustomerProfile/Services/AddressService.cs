using System;
using CustomerProfile.Data;
using CustomerProfile.DTO;
using CustomerProfile.Entities;
using CustomerProfile.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace CustomerProfile.Services;

public class AddressService(UserProfileDbContext dbContext)
{
    private readonly UserProfileDbContext _dbContext = dbContext;

    public async Task<ApiResponse<AddressResponse>> AddAddressAsync(
        AddAddressRequest request,
        CancellationToken ct
    )
    {
        var userExists = await _dbContext.UserProfiles.AnyAsync(u => u.Id == request.UserId, ct);
        if (!userExists)
        {
            return ApiResponse<AddressResponse>.Error("User not found");
        }

        var address = new Address
        {
            UserProfileId = request.UserId,
            AddressType = request.AddressType,
            BuildingNumber = request.BuildingNumber,
            Street = request.Street,
            Landmark = request.Landmark,
            City = request.City,
            LocalGovernmentArea = request.LocalGovernmentArea,
            State = request.State,
            Country = request.Country,
            PostalCode = request.PostalCode,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        _dbContext.Set<Address>().Add(address);
        await _dbContext.SaveChangesAsync(ct);

        return ApiResponse<AddressResponse>.Success(MapToResponse(address));
    }

    public async Task<ApiResponse<List<AddressResponse>>> GetUserAddressesAsync(
        Guid userId,
        CancellationToken ct
    )
    {
        var addresses = await _dbContext
            .Set<Address>()
            .Where(a => a.UserProfileId == userId && !a.IsDeleted)
            .AsNoTracking()
            .ToListAsync(ct);

        return ApiResponse<List<AddressResponse>>.Success(addresses.Select(MapToResponse).ToList());
    }

    public async Task<ApiResponse<AddressResponse>> GetAddressByIdAsync(
        Guid addressId,
        CancellationToken ct
    )
    {
        var address = await _dbContext
            .Set<Address>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == addressId && !a.IsDeleted, ct);

        if (address is null)
        {
            return ApiResponse<AddressResponse>.Error("Address not found");
        }

        return ApiResponse<AddressResponse>.Success(MapToResponse(address));
    }

    public async Task<ApiResponse<AddressResponse>> UpdateAddressAsync(
        Guid addressId,
        UpdateAddressRequest request,
        CancellationToken ct
    )
    {
        var address = await _dbContext
            .Set<Address>()
            .FirstOrDefaultAsync(a => a.Id == addressId && !a.IsDeleted, ct);

        if (address is null)
        {
            return ApiResponse<AddressResponse>.Error("Address not found");
        }

        if (request.UserId != Guid.Empty && address.UserProfileId != request.UserId)
        {
            return ApiResponse<AddressResponse>.Error("Address does not belong to user");
        }

        address.AddressType = request.AddressType;
        address.BuildingNumber = request.BuildingNumber;
        address.Street = request.Street;
        address.Landmark = request.Landmark;
        address.City = request.City;
        address.LocalGovernmentArea = request.LocalGovernmentArea;
        address.State = request.State;
        address.Country = request.Country;
        address.PostalCode = request.PostalCode;
        address.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(ct);

        return ApiResponse<AddressResponse>.Success(MapToResponse(address));
    }

    public async Task<ApiResponse<string>> DeleteAddressAsync(
        Guid addressId,
        Guid userId,
        CancellationToken ct
    )
    {
        var address = await _dbContext
            .Set<Address>()
            .FirstOrDefaultAsync(a => a.Id == addressId && !a.IsDeleted, ct);

        if (address is null)
        {
            return ApiResponse<string>.Error("Address not found");
        }

        if (address.UserProfileId != userId)
        {
            return ApiResponse<string>.Error("Unauthorized");
        }

        address.MarkAsDeleted("User");
        await _dbContext.SaveChangesAsync(ct);

        return ApiResponse<string>.Success("Address deleted successfully");
    }

    private static AddressResponse MapToResponse(Address address)
    {
        return new AddressResponse
        {
            Id = address.Id,
            UserProfileId = address.UserProfileId,
            AddressType = address.AddressType,
            BuildingNumber = address.BuildingNumber,
            Street = address.Street,
            Landmark = address.Landmark,
            City = address.City,
            LocalGovernmentArea = address.LocalGovernmentArea,
            State = address.State,
            Country = address.Country,
            PostalCode = address.PostalCode,
            FullAddress = address.FullAddress,
            IsVerified = address.IsVerified,
        };
    }
}

public class AddAddressRequest
{
    public Guid UserId { get; set; }
    public AddressType AddressType { get; set; }
    public string BuildingNumber { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Landmark { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string LocalGovernmentArea { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
}

public class UpdateAddressRequest
{
    public Guid UserId { get; set; }
    public AddressType AddressType { get; set; }
    public string BuildingNumber { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Landmark { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string LocalGovernmentArea { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
}

public class AddressResponse
{
    public Guid Id { get; set; }
    public Guid UserProfileId { get; set; }
    public AddressType AddressType { get; set; }
    public string BuildingNumber { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Landmark { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string LocalGovernmentArea { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string FullAddress { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
}
