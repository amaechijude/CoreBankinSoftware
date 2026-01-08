using TransactionService.NIBBS.XmlQueryAndResponseBody;

namespace TransactionService.NIBBS;

public interface INibssService
{
    Task<(NESingleResponse? data, string error)> NameEnquiryAsync(
        NESingleRequest request,
        CancellationToken ct
    );

    Task<(FTSingleDebitResponse? data, string error)> FundTransferDebitAsync(
        FTSingleDebitRequest request,
        CancellationToken ct
    );

    Task<(TSQuerySingleResponse? data, string error)> TransactionStatusQueryAsync(
        TSQuerySingleRequest request,
        CancellationToken ct
    );

    Task<(BalanceEnquiryResponse? data, string? error)> BalanceEnquiryAsync(
        BalanceEnquiryRequest request,
        CancellationToken ct
    );

    Task<(FTSingleCreditResponse? data, string error)> FundTransferCreditAsync(
        FTSingleCreditRequest request,
        CancellationToken ct
    );
}
