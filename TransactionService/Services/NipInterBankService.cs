using FluentValidation;
using TransactionService.Data;
using TransactionService.DTOs.NipInterBank;
using TransactionService.Entity;
using TransactionService.Entity.Enums;
using TransactionService.NIBBS;
using TransactionService.NIBBS.XmlQueryAndResponseBody;
using TransactionService.Utils;

namespace TransactionService.Services;

public sealed class NipInterBankService(
    TransactionDbContext dbContext,
    INibssService nibssService,
    IValidator<NameEnquiryRequest> nameEnquiryValidator,
    IValidator<FundCreditTransferRequest> fundCreditTransferValidator
)
{
    private readonly INibssService _nibssService = nibssService;
    private readonly TransactionDbContext _dbContext = dbContext;
    private readonly IValidator<NameEnquiryRequest> _nameEnquiryValidator = nameEnquiryValidator;
    private readonly IValidator<FundCreditTransferRequest> _fundCreditTransferValidator =
        fundCreditTransferValidator;

    public async Task<ApiResultResponse<NameEnquiryResponse>> GetBeneficiaryAccountDetails(
        NameEnquiryRequest request,
        CancellationToken ct
    )
    {
        var validationResult = await _nameEnquiryValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return ApiResultResponse<NameEnquiryResponse>.Error(string.Join("; ", errors));
        }
        // Get Bank Code
        var bankCode = !string.IsNullOrWhiteSpace(request.DestinationBankNubanCode)
            ? request.DestinationBankNubanCode
            : BankCodes.GetBankCode(request.DestinationBankName);
        if (bankCode == null)
        {
            return ApiResultResponse<NameEnquiryResponse>.Error("Bank not supported");
        }

        var sessionId = TransactionIdGenerator.GenerateSessionId(
            request.SenderBankNubanCode,
            request.DestinationBankNubanCode
        );

        var nESingleRequest = new NESingleRequest
        {
            SessionID = sessionId,
            DestinationBankCode = request.DestinationBankNubanCode,
            ChannelCode = "1", // mobile channel code; adjust as necessary
            AccountNumber = request.DestinationAccountNumber,
        };
        var (data, error) = await _nibssService.NameEnquiryAsync(nESingleRequest, ct);
        if (data is null)
        {
            return ApiResultResponse<NameEnquiryResponse>.Error(
                error ?? "Account name enquiry failed"
            );
        }

        if (data.ResponseCode != "00")
        {
            return ApiResultResponse<NameEnquiryResponse>.Error(
                NibssResponseCodesHelper.GetMessageForCode(data.ResponseCode)
            );
        }
        // Successful response
        return ApiResultResponse<NameEnquiryResponse>.Success(
            new NameEnquiryResponse(
                AccountNumber: data.AccountNumber,
                AccountName: data.AccountName,
                BankCode: data.DestinationBankCode,
                BankName: request.DestinationBankName
            )
        );
    }

    public async Task<ApiResultResponse<FundCreditTransferResponse>> FundCreditTransfer(
        Guid customerId,
        FundCreditTransferRequest request,
        CancellationToken cancellationToken
    )
    {
        // Validate Request
        var validationResult = await _fundCreditTransferValidator.ValidateAsync(
            request,
            cancellationToken
        );
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return ApiResultResponse<FundCreditTransferResponse>.Error(string.Join("; ", errors));
        }

        var sessionId = TransactionIdGenerator.GenerateSessionId(
            request.SenderBankNubanCode,
            request.DestinationBankNubanCode
        );
        var fctRequest = new FTSingleCreditRequest
        {
            SessionID = sessionId,
            DestinationBankCode = request.DestinationBankNubanCode,
            ChannelCode = "1", // mobile channel code; adjust as necessary
            AccountName = request.DestinationBankName,
            AccountNumber = request.DestinationAccountNumber,
            OriginatorName = request.SenderAccountName,
            Narration = request.Narration ?? "N/A",
            PaymentReference = request.IdempotencyKey,
            Amount = request.Amount,
        };
        // record in database
        var transactionData = TransactionData.Create(
            request: request,
            transactionType: TransactionType.Credit,
            reference: $"TXN{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
            category: TransactionCategory.NIP_SINGLE_CREDIT,
            sessionId: sessionId
        );
        // Add the initial record to the context before the external call.
        _dbContext.Transactions.Add(transactionData);

        var (data, error) = await _nibssService.FundTransferCreditAsync(
            fctRequest,
            cancellationToken
        );
        if (data is null)
        {
            transactionData.UpdateStatus(TransactionStatus.Failed, error);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return ApiResultResponse<FundCreditTransferResponse>.Error(
                error ?? "Fund credit transfer failed"
            );
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

        return ApiResultResponse<FundCreditTransferResponse>.Success(
            MapToFundCreditTransferResponse(data, request, code)
        );
    }

    private static FundCreditTransferResponse MapToFundCreditTransferResponse(
        FTSingleCreditResponse data,
        FundCreditTransferRequest request,
        string status
    )
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
