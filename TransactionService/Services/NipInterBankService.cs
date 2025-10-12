using Grpc.Core;
using SharedGrpcContracts.Protos.Account.V1;
using TransactionService.Data;
using TransactionService.DTOs.NipInterBank;
using TransactionService.Entity;
using TransactionService.Entity.Enums;
using TransactionService.NIBBS;
using TransactionService.NIBBS.XmlQueryAndResponseBody;
using TransactionService.Utils;

namespace TransactionService.Services;

public class NipInterBankService
    (
    NibssService nibssService,
    TransactionDbContext dbContext,
    ILogger<NipInterBankService> logger,
    AccountGrpcApiService.AccountGrpcApiServiceClient accountGrpcClient
    )
{
    private readonly NibssService _nibssService = nibssService;
    private readonly TransactionDbContext _dbContext = dbContext;
    private readonly ILogger<NipInterBankService> _logger = logger;
    private readonly AccountGrpcApiService.AccountGrpcApiServiceClient _accountGrpcClient = accountGrpcClient;
    private static readonly int TRAANSACTION_FEE = 50; // Flat fee for demo purposes
    private static readonly int MINIMUM_BALANCE = 50;

    public async Task<ApiResultResponse<NameEnquiryResponse>> GetBeneficiaryAccountDetails(NameEnquiryRequest request)
    {
        // Validate Request
        var validator = new NameEnquiryValidator();
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return ApiResultResponse<NameEnquiryResponse>.Error(string.Join("; ", errors));
        }
        // Get Bank Code
        string? bankCode = !string.IsNullOrWhiteSpace(request.DestinationBankNubanCode)
            ? request.DestinationBankNubanCode
            : BankCodes.GetBankCode(request.DestinationBankName);
        if (bankCode == null)
            return ApiResultResponse<NameEnquiryResponse>.Error("Bank not supported");

        var sessionId = TransactionIdGenerator.GenerateSessionId(request.SenderBankNubanCode, request.DestinationBankNubanCode);
        
            var nESingleRequest = new NESingleRequest
            {
                SessionID = sessionId,
                DestinationBankCode = request.DestinationBankNubanCode,
                ChannelCode = "1", // mobile channel code; adjust as necessary
                AccountNumber = request.DestinationAccountNumber
            };
            var (data, error) = await _nibssService.NameEnquiryAsync(nESingleRequest);
            if (data is null)
                return ApiResultResponse<NameEnquiryResponse>.Error(error ?? "Account name enquiry failed");
            if (data.ResponseCode != "00")
                return ApiResultResponse<NameEnquiryResponse>.Error(NibssResponseCodesHelper.GetMessageForCode(data.ResponseCode));
            // Successful response
            return ApiResultResponse<NameEnquiryResponse>.Success(new NameEnquiryResponse
            (
                AccountNumber: data.AccountNumber,
                AccountName: data.AccountName,
                BankCode: data.DestinationBankCode,
                BankName: request.DestinationBankName
            ));
    }

    public async Task<ApiResultResponse<decimal>> GetAccountBalance(string customerId)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            return ApiResultResponse<decimal>.Error("CustomerId is required");

        var (accountResponse, error) = await GetAccountBalanceFromAccountService(customerId);
        if (accountResponse is null)
            return ApiResultResponse<decimal>.Error(error);

        return ApiResultResponse<decimal>.Success((decimal)accountResponse.AccountBalance);
    }

    public async Task<ApiResultResponse<FundCreditTransferResponse>> FundCreditTransfer(Guid customerId, FundCreditTransferRequest request, string indempotencyKey, CancellationToken cancellationToken = default)
    {
        // Validate Request
        var validator = new FundCreditTransferValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return ApiResultResponse<FundCreditTransferResponse>.Error(string.Join("; ", errors));
        }

        // Get Sender Account Details
        var (senderAccountResponse, senderError) = await GetAccountBalanceFromAccountService(request.SenderAccountNumber);
        if (senderAccountResponse is null)
            return ApiResultResponse<FundCreditTransferResponse>.Error(senderError ?? "Sender account validation failed.");

        // Check Sufficient Balance
        if (senderAccountResponse.AccountBalance < (double)request.Amount + TRAANSACTION_FEE + MINIMUM_BALANCE)
            return ApiResultResponse<FundCreditTransferResponse>.Error("Insufficient funds.");



        var sessionId = TransactionIdGenerator.GenerateSessionId(request.SenderBankNubanCode, request.DestinationBankNubanCode);
        var fctRequest = new FTSingleCreditRequest
        {
            SessionID = sessionId,
            DestinationBankCode = request.DestinationBankNubanCode,
            ChannelCode = "1", // mobile channel code; adjust as necessary
            AccountName = request.DestinationBankName,
            AccountNumber = request.DestinationAccountNumber,
            OriginatorName = request.SenderAccountName,
            Narration = request.Narration ?? "N/A",
            PaymentReference = indempotencyKey,
            Amount = request.Amount
        };
        // record in database
        var transactionData = TransactionData.Create(
            idempotencyKey: indempotencyKey,
            customerId: customerId,
            sessionId: sessionId,
            refrence: $"TXN{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
            request: request,
            transactionType: TransactionType.Credit,
            transactionChannel: TransactionChannel.MobileApp,
            transactionCategory: TransactionCategory.NIP_INTER_BANK_TRANSFER,
            transactionStatus: TransactionStatus.Initiated
        );
        // Add the initial record to the context before the external call.
        await _dbContext.Transactions.AddAsync(transactionData, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var (data, error) = await _nibssService.FundTransferCreditAsync(fctRequest);
        if (data is null)
        {
            transactionData.UpdateStatus(TransactionStatus.Failed, error);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return ApiResultResponse<FundCreditTransferResponse>.Error(error ?? "Fund credit transfer failed");
        }

        var code = NibssResponseCodesHelper.GetMessageForCode(data.ResponseCode);
        var status = NibssResponseCodesHelper.GetTransactionStatus(data.ResponseCode);

        // The correct logic is to check if the code is NOT "00" AND NOT "09".
        if (data.ResponseCode != "00" && data.ResponseCode != "09")
        {
            transactionData.UpdateStatus(status, code);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return ApiResultResponse<FundCreditTransferResponse>.Error(code);
        }

        // This handles the success ("00") and pending ("09") cases.
        transactionData.UpdateStatus(status, code);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ApiResultResponse<FundCreditTransferResponse>
                .Success(MapToFundCreditTransferResponse(data, request, code));
    }

    private async Task<(GetAccountResponse?, string error)> GetAccountBalanceFromAccountService(string customerId)
    {
        try
        {
            var request = new GetAccountByCustomerIdRequest { CustomerId = customerId };
            var accountResponse = await _accountGrpcClient.GetAccountByByCustomerIdAsync(request);
            if (accountResponse is null)
                return (null, "No response from Account service.");

            // General failure check first
            if (!accountResponse.Success)
                return (null, accountResponse.Error ?? "Account not found");
            // Then, check account status
            if (!accountResponse.IsActive)
                return (null, accountResponse.Error ?? "Account is not active");
            if (!accountResponse.CanTransact)
                return (null, accountResponse.Error ?? "Account is restricted from transactions");
            if (accountResponse.AccountBalance < 0)
                return (null, "Insufficient account balance");

            return (accountResponse, string.Empty);
        }
        catch (RpcException rpcEx)
        {
            _logger.LogError(rpcEx, "gRPC error while fetching account details for CustomerId: {CustomerId}", customerId);
            return (null, "Account service unavailable");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching account details for CustomerId: {CustomerId}", customerId);
            return (null, "Internal server error");
        }
    }

    private static FundCreditTransferResponse MapToFundCreditTransferResponse(FTSingleCreditResponse data, FundCreditTransferRequest request, string status)
    {
        return new FundCreditTransferResponse(
            Amount: request.Amount,
            Status: status,
            TransactionDateTime: DateTime.Now,

            SenderAccountNumber: request.SenderAccountNumber,
            SenderBankName: request.SenderBankName,
            SenderAccountName: request.SenderAccountName,

            BeneficiaryAccountNumber: request.DestinationAccountNumber,
            BeneficiaryBankName: request.DestinationBankName,
            BeneficiaryAccountName: data.AccountName ?? "N/A",

            Narration: request.Narration ?? "N/A",
            SessionID: data.SessionID,
            TransactionReference: data.PaymentReference
        );
    }
}
