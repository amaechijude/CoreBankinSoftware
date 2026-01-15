using System;
using CustomerProfile.Data;

namespace CustomerProfile.Services;

public class AddressService(UserProfileDbContext dbContext)
{
    private readonly UserProfileDbContext _dbContext = dbContext;

    public async Task<AddAddresResponse> AddAddressAsync(
        AddAddressRequest request,
        CancellationToken ct
    ) { }
}

public class AddAddresResponse { }

public class AddAddressRequest { }
