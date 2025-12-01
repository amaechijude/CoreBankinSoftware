using Grpc.Core;
using SharedGrpcContracts.Protos.Account.V1;
using TransactionService.Data;
using TransactionService.DTOs.Intrabank;
using TransactionService.DTOs.NipInterBank;
using TransactionService.Entity;
using TransactionService.Utils;

namespace TransactionService.Services;

internal class IntraBankService(
    TransactionDbContext dbContext,
    ILogger<NipInterBankService> logger,
    AccountGrpcApiService.AccountGrpcApiServiceClient accountGrpcClient
    )
{

    public async Task<ApiResultResponse<IntraBankNameEnquiryResponse>> IntraNameEnquiryAsync(IntraBankNameEnquiryRequest request, CancellationToken cancellationToken)
    {
        var validator = new IntraBankNameEnquiryRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return ApiResultResponse<IntraBankNameEnquiryResponse>.Error(string.Join(" ", errors));
        }
        var query = await GetDestinationAccountByAccountNumberAsync(request.AccountNumber);
        if (query is null)
            return ApiResultResponse<IntraBankNameEnquiryResponse>.Error("Account not found");

        return ApiResultResponse<IntraBankNameEnquiryResponse>
            .Success(new IntraBankNameEnquiryResponse(
            AccountName: query.AccountName,
            AccountNuber: query.AccountNumber,
            BankName: query.BankName
        ));
    }
    public async Task<ApiResultResponse<IntraBankTransferResponse>> HandleTransferRequestAsync(IntraBankTransferRequest request, CancellationToken ct)
    {
        var max_retries = 4;
        List<TransactionData> transactions = [
            TransactionData.Create()
        ];

    }


    // public async Task<ApiResultResponse<FundCreditTransferResponse>> IntraFundCreditTransferAsync(FundCreditTransferRequest request, Guid customerId, CancellationToken cancellationToken)
    // {
    //     // Validate Request
    //     var validator = new FundCreditTransferValidator();
    //     var validationResult = await validator.ValidateAsync(request, cancellationToken);
    //     if (!validationResult.IsValid)
    //     {
    //         var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
    //         return ApiResultResponse<FundCreditTransferResponse>.Error(string.Join("; ", errors));
    //     }

    //     var sessionId = TransactionIdGenerator.GenerateSessionId(request.SenderAccountNumber, request.DestinationAccountNumber);

    //     var task1 = GetAccountByCustomerIdAsync(customerId);
    //     var task2 = GetDestinationAccountByAccountNumberAsync(request.SenderAccountNumber);

    //     var getCustomerAccountResponse = await task1;
    //     var getDestinationAccountResponse = await task2;

    //     if (getCustomerAccountResponse is null || getDestinationAccountResponse is null)
    //         return ApiResultResponse<FundCreditTransferResponse>.Error("Account not found");

    //     return ApiResultResponse<FundCreditTransferResponse>.Error(" ");
    // }

    private async Task<GetAccountResponse?> GetAccountByCustomerIdAsync(Guid customerId)
    {
        var request = new GetAccountByCustomerIdRequest { CustomerId = customerId.ToString() };

        try
        {
            return await accountGrpcClient.GetAccountByByCustomerIdAsync(request);
        }
        catch (RpcException ex)
        {
            // retry and log
            logger.LogError(ex, "Account Service unavailable");
            return null;
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(ex, "Account Enquiry for {customerId} failed", customerId.ToString());
            }
            return null;
        }
    }

    private async Task<GetAccountResponse?> GetDestinationAccountByAccountNumberAsync(string accountNumber)
    {
        var request = new GetAccountByAccountNumberRequest { AccountNumber = accountNumber };

        try
        {
            var response = await accountGrpcClient.GetAccountByAccountNumberAsync(request);
            return response;
        }
        catch (RpcException ex)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(ex, "Account Service unavailable");
            }
            return null;
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(ex, "Account Enquiry for {accountNumber} failed", accountNumber);
            }
            return null;
        }
    }

}
