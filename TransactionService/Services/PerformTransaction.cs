using TransactionService.DTOs;
using TransactionService.NIBBS;
using TransactionService.NIBBS.XmlQueryAndResponseBody;
using TransactionService.Utils;

namespace TransactionService.Services;

public class PerformTransaction(NibssService nibssService, NubanAccountLookUp nubanAccountLookUp, ILogger<PerformTransaction> logger)
{
    private readonly NibssService _nibssService = nibssService;
    private readonly NubanAccountLookUp _nubanAccountLookUp = nubanAccountLookUp;
    private readonly ILogger<PerformTransaction> _logger = logger;

    public async Task<ApiResultResponse<NESingleResponse>> GetBeneficiaryAccountDetails(NameEnquiryRequest request)
    {
        string? bankCode = !string.IsNullOrWhiteSpace(request.DestinationBankNubanCode)
            ? request.DestinationBankNubanCode
            : BankCodes.GetBankCode(request.DestinationBankName);
        if (bankCode == null)
            return ApiResultResponse<NESingleResponse>.Error("Bank not supported");

        var sessionId = TransactionIdGenerator.GenerateSessionId(request.SenderBankNubanCode, request.DestinationBankNubanCode);
        try
        {
            var (data, error) = await _nibssService.NameEnquiryAsync(new NESingleRequest
            {
                SessionID = sessionId,
                DestinationBankCode = request.DestinationBankNubanCode,
                ChannelCode = "1", // mobile channel code; adjust as necessary
                AccountNumber = request.DestinationAccountNumber
            });
            if (data is not null && data.ResponseCode == "00")
                return ApiResultResponse<NESingleResponse>.Success(data);

            return ApiResultResponse<NESingleResponse>.Error("Account not found");
        }
        catch (HttpRequestException)
        {
            // fallback to local NUBAN lookup
            _logger.LogWarning("Nibss name enquiry failed, falling back to local NUBAN lookup");
            return await GetBeneficiaryAccountDetailsFallback(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during NIBSS name enquiry");
            return ApiResultResponse<NESingleResponse>.Error("Internal server error");
        }
    }

    private async Task<ApiResultResponse<NESingleResponse>> GetBeneficiaryAccountDetailsFallback(NameEnquiryRequest request)
    {
        var accountDetails = await _nubanAccountLookUp.GetAccountDetails(request.DestinationAccountNumber, request.DestinationBankNubanCode);
        if (accountDetails is null)
            return ApiResultResponse<NESingleResponse>.Error("Account not found");

        return ApiResultResponse<NESingleResponse>.Success(new NESingleResponse
        {
            AccountName = accountDetails.AccountName,
            AccountNumber = accountDetails.AccountNumber,
            DestinationBankCode = accountDetails.BankCode,
            SessionID = $"Fallback {DateTime.UtcNow:yyyyMMddHHmm}",
            ChannelCode = "1", // mobile channel code; adjust as necessary
            ResponseCode = "00"
        });

    }
}

