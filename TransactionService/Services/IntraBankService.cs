using Grpc.Core;
using SharedGrpcContracts.Protos.Account.V1;
using TransactionService.Data;
using TransactionService.DTOs.Intrabank;
using TransactionService.DTOs.NipInterBank;
using TransactionService.Entity;
using TransactionService.Entity.Enums;
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
        var query = await GetAccountDetails(request.AccountNumber);
        if (query is null)
            return ApiResultResponse<IntraBankNameEnquiryResponse>.Error("Account not found");

        return ApiResultResponse<IntraBankNameEnquiryResponse>
            .Success(query);
    }


    public async Task<ApiResultResponse<object>> IntraFundCreditTransferAsync(FundCreditTransferRequest request, Guid customerId, CancellationToken cancellationToken)
    {
        // Validate Request
        var validator = new FundCreditTransferValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return ApiResultResponse<object>.Error(string.Join("; ", errors));
        }
        var query = await GetAccountDetails(customerId);
        if (query is null)
            return ApiResultResponse<object>.Error("Unable to proceed with transfer, Try again later");

        if (!query.AccountBalance.HasValue || (decimal)query.AccountBalance + 100 < request.Amount)
            return ApiResultResponse<object>.Error("Insufficient Funds");

        var sessionId = TransactionIdGenerator
            .GenerateSessionId(request.SenderAccountNumber, request.DestinationAccountNumber);

        // record in database
        var transactionData = TransactionData.Create(
            request: request,
            transactionType: TransactionType.Transfer,
            reference: $"TXN{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
            category: TransactionCategory.INTRA_BANK_TRANSFER,
            sessionId: sessionId
        );
        var outbox = OutboxMessage.Create(transactionData);

        dbContext.Transactions.Add(transactionData);
        dbContext.OutboxMessages.Add(outbox);

        await dbContext.SaveChangesAsync(cancellationToken);
        return ApiResultResponse<object>.Success(new { status = "Transfer processing" });
    }

    private async Task<IntraBankNameEnquiryResponse?> GetAccountDetails(Guid customerId)
    {
        try
        {
            var request = new GetAccountByCustomerIdRequest { CustomerId = customerId.ToString() };
            var query = await accountGrpcClient.GetAccountByByCustomerIdAsync(request);
            return new IntraBankNameEnquiryResponse(
            AccountName: query.AccountName,
            AccountNuber: query.AccountNumber,
            BankName: query.BankName,
            AccountBalance: query.AccountBalance
        );
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

    private async Task<IntraBankNameEnquiryResponse?> GetAccountDetails(string accountNumber)
    {
        var request = new GetAccountByAccountNumberRequest { AccountNumber = accountNumber };

        try
        {
            var query = await accountGrpcClient.GetAccountByAccountNumberAsync(request);
            return new IntraBankNameEnquiryResponse(
            AccountName: query.AccountName,
            AccountNuber: query.AccountNumber,
            BankName: query.BankName,
            AccountBalance: null
        );
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
