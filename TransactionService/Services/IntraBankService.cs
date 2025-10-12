using Grpc.Core;
using SharedGrpcContracts.Protos.Account.V1;
using TransactionService.Data;
using TransactionService.DTOs.NipInterBank;
using TransactionService.Utils;

namespace TransactionService.Services;

public class IntraBankService(
    TransactionDbContext dbContext,
    ILogger<NipInterBankService> logger,
    AccountGrpcApiService.AccountGrpcApiServiceClient accountGrpcClient
    )
{
    private readonly TransactionDbContext _dbContext = dbContext;
    private readonly ILogger<NipInterBankService> _logger = logger;
    private readonly AccountGrpcApiService.AccountGrpcApiServiceClient _accountGrpcClient = accountGrpcClient;

    private static readonly int TRAANSACTION_FEE = 50; // Flat fee for demo purposes
    private static readonly int MINIMUM_BALANCE = 50;

    public async Task<ApiResultResponse<FundCreditTransferResponse>> IntraFundCreditTransferAsync(FundCreditTransferRequest request, Guid CustomerId, CancellationToken cancellationToken = default)
    {
        // Validate Request
        var validator = new FundCreditTransferValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return ApiResultResponse<FundCreditTransferResponse>.Error(string.Join("; ", errors));
        }
        var sessionId = TransactionIdGenerator.GenerateSessionId(request.SenderAccountNumber, request.DestinationAccountNumber);

        var getCustomerAccountResponse = await GetAccountByCustomerIdAsync(CustomerId);
        var getDestinationAccountResponse = await GetDestinationAccountByAccountNumberAsync(request.SenderAccountNumber);

        if (getCustomerAccountResponse is null || getDestinationAccountResponse is null)
            return ApiResultResponse<FundCreditTransferResponse>.Error("Account not found");

        return ApiResultResponse<FundCreditTransferResponse>.Error("Not implemented");
    }

    private async Task<GetAccountResponse?> GetAccountByCustomerIdAsync(Guid customerId)
    {
        var request = new GetAccountByCustomerIdRequest { CustomerId = customerId.ToString() };

        try
        {
            GetAccountResponse? response = await _accountGrpcClient.GetAccountByByCustomerIdAsync(request);
            return response;
        }
        catch (RpcException ex)
        {
            // retry and log
            _logger.LogError(ex, "Account Service unavailable");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Account Enquiry for {customerId} failed", customerId.ToString());
            return null;
        }
    }

    private async Task<GetAccountResponse?> GetDestinationAccountByAccountNumberAsync(string accountNumber)
    {
        var request = new GetAccountByAccountNumberRequest { AccountNumber = accountNumber };

        try
        {
            GetAccountResponse? response = await _accountGrpcClient.GetAccountByAccountNumberAsync(request);
            return response;
        }
        catch (RpcException ex)
        {
            // log and retry
            _logger.LogError(ex, "Account Service unavailable");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Account Enquiry for {accountNumber} failed", accountNumber);
            return null;
        }
    }
    
}
