using Grpc.Core;
using SharedGrpcContracts.Protos.Account.V1;
using TransactionService.DTOs;
using TransactionService.NIBBS;
using TransactionService.NIBBS.XmlQueryAndResponseBody;
using TransactionService.Utils;

namespace TransactionService.Services;

public class PerformTransaction
    (
    NibssService nibssService,
    // NubanAccountLookUp nubanAccountLookUp,
    ILogger<PerformTransaction> logger,
    AccountGrpcApiService.AccountGrpcApiServiceClient accountGrpcClient
    )
{
    private readonly NibssService _nibssService = nibssService;
    // private readonly NubanAccountLookUp _nubanAccountLookUp = nubanAccountLookUp;
    private readonly ILogger<PerformTransaction> _logger = logger;
    private readonly AccountGrpcApiService.AccountGrpcApiServiceClient _accountGrpcClient = accountGrpcClient;
    private static readonly int TRAANSACTION_FEE = 50; // Flat fee for demo purposes

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
        try
        {
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
        catch (HttpRequestException)
        {
            // fallback to local NUBAN lookup
            _logger.LogWarning("Nibss name enquiry failed, falling back to local NUBAN lookup");
            return ApiResultResponse<NameEnquiryResponse>.Error("Internal server error");
        }
        catch (TimeoutException)
        {
            // fallback to local NUBAN lookup
            _logger.LogWarning("Nibss name enquiry timed out, falling back to local NUBAN lookup");
            return ApiResultResponse<NameEnquiryResponse>.Error("Internal server error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during NIBSS name enquiry");
            return ApiResultResponse<NameEnquiryResponse>.Error("Internal server error");
        }
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

    public async Task<ApiResultResponse<FundCreditTransferResponse>> FundCreditTransfer(FundCreditTransferRequest request, string indempotencyKey, CancellationToken cancellationToken=default)
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
            return ApiResultResponse<FundCreditTransferResponse>.Error($"Sender account error: {senderError}");

        // Check Sufficient Balance
        if (senderAccountResponse.AccountBalance < (double)request.Amount + TRAANSACTION_FEE)
            return ApiResultResponse<FundCreditTransferResponse>.Error("Insufficient funds in sender's account");

        // Get Bank Code
        string? bankCode = !string.IsNullOrWhiteSpace(request.DestinationBankNubanCode)
            ? request.DestinationBankNubanCode
            : BankCodes.GetBankCode(request.DestinationBankName);
        if (bankCode == null)
            return ApiResultResponse<FundCreditTransferResponse>.Error("Destination bank not supported");

        var sessionId = TransactionIdGenerator.GenerateSessionId(request.SenderBankNubanCode, request.DestinationBankNubanCode);
        try
        {
            var fctRequest = new FTSingleCreditRequest
            {
                SessionID = sessionId,
                DestinationBankCode = request.DestinationBankNubanCode,
                ChannelCode = "1", // mobile channel code; adjust as necessary
                AccountName = request.DestinationBankName,
                AccountNumber = request.DestinationAccountNumber,
                OriginatorName = request.SenderBankName,
                Narration = request.Narration ?? "N/A",
                PaymentReference = indempotencyKey,
                Amount = request.Amount
            };
            var (data, error) = await _nibssService.FundTransferCreditAsync(fctRequest);
            if (data is null)
                return ApiResultResponse<FundCreditTransferResponse>.Error(error ?? "Fund credit transfer failed");
            if (data.ResponseCode != "00")
                return ApiResultResponse<FundCreditTransferResponse>.Error(NibssResponseCodesHelper.GetMessageForCode(data.ResponseCode));

            // Successful response
            // return ApiResultResponse<FundCreditTransferResponse>.Success(new FundCreditTransferResponse
            // {
            //     Amount = request.Amount,
            //     Status = data.ResponseCode,
            //     TransactionDateTime = DateTime.Now,
            //     SenderAccountNumber = request.SenderAccountNumber,
            //     SenderBankName = request.SenderBankName,
            //     SenderAccountName = senderAccountResponse.AccountName,
            //     BeneficiaryAccountNumber = request.DestinationAccountNumber,
            //     BeneficiaryBankName = request.DestinationBankName,
            //     BeneficiaryAccountName = data.BeneficiaryAccountName ?? "N/A",
            //     Narration = request.Narration ?? "N/A",
            //     SessionID = sessionId,
            //     TransactionReference = data.TransactionReference
            // });
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
}
