using Grpc.Core;
using SharedGrpcContracts.Protos.Account.V1;


namespace CustomerAPI.Services.AccountAPI;

public class AccountApiCall(
    AccountGrpcApiService.AccountGrpcApiServiceClient grpcClient,
    ILogger<AccountApiCall> logger)
{
    public async Task<CreateAccountResponseDto> CreateAccountGrpc(string id)
    {
        try
        {
            logger.LogInformation("Attempting to create account via gRPC for CustomerId: {CustomerId}", id);

            var response = await grpcClient.CreateAccountAsync(new CreateAccountRequest { CustomerId = id });

            // Assuming the .proto file defines a response with a success flag.
            // If not, the absence of an exception indicates success.
            if (response != null && response.Success)
            {
                logger.LogInformation("Successfully created account for CustomerId: {CustomerId}", id);
                return CreateAccountResponseDto.SuccessResponse(response);
            }

            logger.LogWarning("gRPC call to create account for CustomerId: {CustomerId} did not succeed. Response: {@Response}", id, response);
            return CreateAccountResponseDto.ErrorResponse("Failed to create account.");
        }
        catch (RpcException ex)
        {
            logger.LogError(ex, "gRPC error while creating account for CustomerId: {CustomerId}. Status: {StatusCode}", id, ex.StatusCode);
            return CreateAccountResponseDto.ErrorResponse("Account creation Service is currently unavailable.");
        }
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
