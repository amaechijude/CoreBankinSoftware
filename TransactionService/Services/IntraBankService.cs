using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Text.Json;
using FluentValidation;
using Grpc.Core;
using Microsoft.Extensions.Caching.Distributed;
using SharedGrpcContracts.Protos.Account.Operations.V1;
using TransactionService.Data;
using TransactionService.DTOs.IntraBank;
using TransactionService.Entity;
using TransactionService.Entity.Enums;
using TransactionService.Utils;

namespace TransactionService.Services;

public sealed class IntraBankService(
    AccountOperationsGrpcService.AccountOperationsGrpcServiceClient client,
    IDistributedCache distributedCache,
    TransactionDbContext dbContext,
    IValidator<TransferRequestIntra> transferValidator,
    IValidator<NameEnquiryIntraRequest> nameEnquiryValidator,
    ILogger<IntraBankService> logger
)
{
    private readonly AccountOperationsGrpcService.AccountOperationsGrpcServiceClient _accountGrpcClient =
        client;
    private readonly IDistributedCache _distributedCache = distributedCache;
    private readonly TransactionDbContext _dbContext = dbContext;
    private readonly IValidator<TransferRequestIntra> _transferValidator = transferValidator;
    private readonly IValidator<NameEnquiryIntraRequest> _nameEnquiryValidator =
        nameEnquiryValidator;
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
        // check cache
        var cacheKey = $"account_{request.AccountNumber}";
        var dataBytes = await _distributedCache.GetAsync(cacheKey, ct);

        if (dataBytes is not null)
        {
            var responseData = JsonSerializer.Deserialize<NameEnquiryIntraResponse>(dataBytes);
            return ApiResultResponse<NameEnquiryIntraResponse>.Success(responseData!);
        }
        // fallback to grpc call
        var response = await CallGrpcService(request, ct);
        if (response is null)
            return ApiResultResponse<NameEnquiryIntraResponse>.Error("Account not found");

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
        var value = JsonSerializer.SerializeToUtf8Bytes(data);
        await CacheResult(cacheKey, value, ct);
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
        var sessionId = TransactionIdGenerator.GenerateSessionId(
            request.SenderAccountNumber,
            request.DestinationAccountNumber
        );
        var transactionData = TransactionData.Create(
            request: request,
            transactionType: TransactionType.Transfer,
            reference: $"TXN{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
            category: TransactionCategory.INTRA_BANK_TRANSFER,
            sessionId: sessionId
        );
        _dbContext.Transactions.Add(transactionData);
        await _dbContext.SaveChangesAsync(ct); // save initial state to db

        // synchronous call to account service via grpc
        var response = await CallGrpcTransferService(request, ct);

        if (response is null)
        {
            transactionData.UpdateStatus(TransactionStatus.Failed, "Transaction failed");
            await _dbContext.SaveChangesAsync(ct);
            return ApiResultResponse<TransferResponseIntra>.Error("Transfer failed");
        }
        if (!response.Success)
        {
            var error = string.IsNullOrWhiteSpace(response.Error)
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
            SessionID: sessionId,
            TransactionReference: transactionData.TransactionReference
        );

        return ApiResultResponse<TransferResponseIntra>.Success(data);
    }

    private async Task CacheResult(string key, byte[] value, CancellationToken ct)
    {
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20),
            SlidingExpiration = TimeSpan.FromMinutes(15),
        };

        await _distributedCache.SetAsync(key: key, value: value, options: cacheOptions, token: ct);
    }

    private static CallOptions GetCallOptions(CancellationToken ct) =>
        new(deadline: DateTime.UtcNow.AddSeconds(30), cancellationToken: ct);

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
            return response is null ? null : response;
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
            return response is null ? null : response;
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
};
