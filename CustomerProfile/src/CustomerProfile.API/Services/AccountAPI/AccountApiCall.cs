using Microsoft.Extensions.Options;


namespace CustomerAPI.Services.AccountAPI;

public class AccountApiCall(IOptions<AccountApiOptions> options)
{
    
    private readonly string _accountApiUrl = options.Value.AccountApiUrl;
    public async Task<bool> CreateAccountGrpc(string id)
    {
        using var grpcChannel = Grpc.Net.Client.GrpcChannel.ForAddress(_accountApiUrl);
        var grpcClient = new AccountGrpcApiService.AccountGrpcApiServiceClient(grpcChannel);

        var response = await grpcClient
            .CreateAccountAsync(new CreateAccountRequest { CustomerId = id });
        return true;
    }
}

public class CreateAccountResponseDto
{
    public bool Success { get; private set; }
    public CreateAccountResponse? Data { get; private set; }
    public string? ErrorMessage { get; private set; }

    public static CreateAccountResponseDto SuccessResponse(CreateAccountResponse data)
    {
        return new CreateAccountResponseDto { Success = data.Success, Data = data };
    }

    public static CreateAccountResponseDto ErrorResponse(string errorMessage)
    {
        return new CreateAccountResponseDto { Success = false, ErrorMessage = errorMessage };
    }
}
