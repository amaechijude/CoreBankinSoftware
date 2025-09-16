using CustomerProfile.API.Services.AccountAPI.AccountAPI;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;
using SharedGrpcContracts.Protos;


namespace CustomerAPI.Services.AccountAPI;

public class AccountApiCall (IOptions<AccountApiOptions> options)
{
    private readonly string _accountApiUrl = options.Value.AccountApiUrl;
    public async Task<bool> CreateAccountGrpc(string id)
    {
        using var grpcChannel = GrpcChannel.ForAddress(_accountApiUrl);
        var grpcClient = new AccountGrpcApiService.AccountGrpcApiServiceClient(grpcChannel);

        var response = await grpcClient
            .CreateAccountAsync(new CreateAccountRequest { CustomerId = id });
        return true;
    }
}
