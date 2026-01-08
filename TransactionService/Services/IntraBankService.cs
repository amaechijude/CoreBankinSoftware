using System.Threading.Channels;
using FluentValidation;
using Grpc.Core;
using Microsoft.Extensions.Caching.Hybrid;
using SharedGrpcContracts.Protos.Account.Operations.V1;
using TransactionService.Data;
using TransactionService.DTOs.IntraBank;
using TransactionService.Entity;
using TransactionService.Entity.Enums;
using TransactionService.Utils;

namespace TransactionService.Services;

public sealed class IntraBankService(
    AccountOperationsGrpcService.AccountOperationsGrpcServiceClient client,
    HybridCache hybridCache,
    TransactionDbContext dbContext,
    IValidator<TransferRequestIntra> transferValidator,
    IValidator<NameEnquiryIntraRequest> nameEnquiryValidator,
    Channel<OutboxMessage> channel,
    ILogger<IntraBankService> logger
)
{
    private readonly AccountOperationsGrpcService.AccountOperationsGrpcServiceClient _accountGrpcClient =
        client;
    private readonly HybridCache _hybridCache = hybridCache;
    private readonly TransactionDbContext _dbContext = dbContext;
    private readonly IValidator<TransferRequestIntra> _transferValidator = transferValidator;
    private readonly IValidator<NameEnquiryIntraRequest> _nameEnquiryValidator =
        nameEnquiryValidator;
    private readonly Channel<OutboxMessage> _channel = channel;
    private readonly ILogger<IntraBankService> _logger = logger;

    public async Task<ApiResultResponse<NameEnquiryIntraResponse>> NameEnquiry(
        NameEnquiryIntraRequest request,
        CancellationToken ct
    )
    {
        // validate request
        var validationResult = await _nameEnquiryValidator.ValidateAsync(request, cancellation: ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return ApiResultResponse<NameEnquiryIntraResponse>.Error(
                "Invalid Account number",
                errors
            );
        }
        var cacheKey = $"IntraBankNameEnquiry-{request.AccountNumber}";

        IntraBankNameEnquiryResponse? response = await _hybridCache.GetOrCreateAsync(
            key: cacheKey,
            factory: async token => await CallGrpcService(request, token),
            cancellationToken: ct
        );
        if (response is null)
        {
            return ApiResultResponse<NameEnquiryIntraResponse>.Error("Account not found");
        }

        if (!response.Success)
        {
            return ApiResultResponse<NameEnquiryIntraResponse>.Error(
                response.Error ?? "Name enquiry failed"
            );
        }
        var data = new NameEnquiryIntraResponse(
            AccountNumber: response.AccountNumber,
            AccountName: response.AccountName,
            BankCode: "BankCode", // hardcoded for now
            BankName: response.BankName
        );
        return ApiResultResponse<NameEnquiryIntraResponse>.Success(data);
    }

    public async Task<ApiResultResponse<TransferResponseIntra>> Transfer(
        TransferRequestIntra request,
        CancellationToken ct
    )
    {
        // validate request
        var validationResult = await _transferValidator.ValidateAsync(request, cancellation: ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return ApiResultResponse<TransferResponseIntra>.Error(
                "Invalid transfer request",
                errors
            );
        }
        // create transaction in db
        var transactionData = NewTransactionData(request);
        _dbContext.Transactions.Add(transactionData);

        // synchronous call to account service via grpc
        var response = await CallGrpcTransferService(request, ct);

        if (response is null || response.Success == false)
        {
            var error = string.IsNullOrWhiteSpace(response?.Error)
                ? "Transfer failed"
                : response.Error;

            transactionData.UpdateStatus(TransactionStatus.Failed, error);
            await _dbContext.SaveChangesAsync(ct);
            return ApiResultResponse<TransferResponseIntra>.Error(error);
        }

        // update transaction status in db
        transactionData.UpdateStatus(TransactionStatus.Completed, "Transaction completed");
        var outbox = OutboxMessage.Create(transactionData);
        _dbContext.OutboxMessages.Add(outbox);
        await _dbContext.SaveChangesAsync(ct);

        var data = new TransferResponseIntra(
            Amount: request.Amount,
            Status: response.Success ? "Success" : "Failed",
            TransactionDateTime: DateTimeOffset.UtcNow,
            SessionID: transactionData.SessionId,
            TransactionReference: transactionData.TransactionReference
        );

        // await _channel.Writer.WriteAsync(outbox, ct);

        return ApiResultResponse<TransferResponseIntra>.Success(data);
    }

    private static CallOptions GetCallOptions(CancellationToken ct) =>
        new(deadline: DateTime.UtcNow.AddSeconds(20), cancellationToken: ct);

    private async Task<IntraBankNameEnquiryResponse?> CallGrpcService(
        NameEnquiryIntraRequest request,
        CancellationToken ct
    )
    {
        try
        {
            var grpcRequest = new IntraBankNameEnquiryRequest
            {
                AccountNumber = request.AccountNumber,
            };
            var response = await _accountGrpcClient.IntraBankNameEnquiryAsync(
                request: grpcRequest,
                options: GetCallOptions(ct)
            );
            return response;
        }
        catch (RpcException ex)
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError(
                    ex,
                    "Name enquiry failed for {accountNumber}",
                    request.AccountNumber
                );
            }
            return null;
        }
    }

    private async Task<AccountOperationResponse?> CallGrpcTransferService(
        TransferRequestIntra request,
        CancellationToken ct
    )
    {
        try
        {
            var grpcRequest = new TransferRequest
            {
                Amount = (double)request.Amount,
                CustomerId = request.CustomerId.ToString(),
                ToAccountNumber = request.DestinationAccountNumber,
                SessionId = request.SessionId,
            };
            var response = await _accountGrpcClient.TransferAsync(
                request: grpcRequest,
                options: GetCallOptions(ct)
            );
            return response;
        }
        catch (RpcException ex)
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError(
                    ex,
                    "Intra bank transfer failed from {sender} to {beneficiary}",
                    request.CustomerId,
                    request.DestinationAccountNumber
                );
            }
            return null;
        }
    }

    private static TransactionData NewTransactionData(TransferRequestIntra request)
    {
        // create transaction in db
        var sessionId = TransactionIdGenerator.GenerateSessionId(
            request.SenderAccountNumber,
            request.DestinationAccountNumber
        );
        return TransactionData.Create(
            request: request,
            transactionType: TransactionType.Transfer,
            reference: $"TXN{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
            category: TransactionCategory.INTRA_BANK_TRANSFER,
            sessionId: sessionId
        );
    }
};
