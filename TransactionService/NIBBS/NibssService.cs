using Polly;
using Polly.Registry;
using TransactionService.NIBBS.XmlQueryAndResponseBody;
using TransactionService.Utils;

namespace TransactionService.NIBBS;

public sealed class NibssService(
    HttpClient client,
    ResiliencePipelineProvider<string> pipelineProvider
) : INibssService
{
    private readonly HttpClient _client = client;
    private readonly ResiliencePipeline _pipeline = pipelineProvider.GetPipeline("key");

    /// <summary>
    /// Performs a name enquiry using the provided request details.
    /// </summary>
    /// <param name="request">The name enquiry request.</param>
    /// <returns>A tuple containing the name enquiry response on success, or an error message on failure.</returns>
    public async Task<(NESingleResponse? data, string error)> NameEnquiryAsync(
        NESingleRequest request,
        CancellationToken ct
    )
    {
        return await PostXmlAsync<NESingleRequest, NESingleResponse>(
            request,
            "/name-enquiry",
            "Name enquiry failed",
            ct
        );
    }

    /// <summary>
    /// Initiates a single debit fund transfer
    /// </summary>
    /// <param name="request">The fund transfer request details.</param>
    /// <returns>A tuple containing the fund transfer response on success, or an error message on failure.</returns>
    public async Task<(FTSingleDebitResponse? data, string error)> FundTransferDebitAsync(
        FTSingleDebitRequest request,
        CancellationToken ct
    )
    {
        return await PostXmlAsync<FTSingleDebitRequest, FTSingleDebitResponse>(
            request,
            "/debit-fund-transfer",
            "Debit fund transfer failed",
            ct
        );
    }

    /// <summary>
    /// Queries the status of a single transaction.
    /// </summary>
    /// <param name="request">The transaction status query request details.</param>
    /// <returns>A tuple containing the transaction status query response on success, or an error message on failure.</returns>

    public async Task<(TSQuerySingleResponse? data, string error)> TransactionStatusQueryAsync(
        TSQuerySingleRequest request,
        CancellationToken ct
    )
    {
        return await PostXmlAsync<TSQuerySingleRequest, TSQuerySingleResponse>(
            request,
            "/transaction-status-query",
            "TransactionData status query failed",
            ct
        );
    }

    /// <summary>
    /// Performs a balance enquiry using the provided request details.
    /// </summary>
    /// <param name="request">The balance enquiry request.</param>
    /// <returns>A tuple containing the balance enquiry response on success, or an error message on failure.</returns>
    public async Task<(BalanceEnquiryResponse? data, string? error)> BalanceEnquiryAsync(
        BalanceEnquiryRequest request,
        CancellationToken ct
    )
    {
        return await PostXmlAsync<BalanceEnquiryRequest, BalanceEnquiryResponse>(
            request,
            "/balance-enquiry",
            "Balance enquiry failed",
            ct
        );
    }

    /// <summary>
    /// Initiates a single credit fund transfer, which moves funds from a sender's account to a beneficiary's account.
    /// </summary>
    /// <param name="request">The fund transfer request details.</param>
    /// <returns>A tuple containing the fund transfer response on success, or an error message on failure.</returns>
    public async Task<(FTSingleCreditResponse? data, string error)> FundTransferCreditAsync(
        FTSingleCreditRequest request,
        CancellationToken ct
    )
    {
        return await PostXmlAsync<FTSingleCreditRequest, FTSingleCreditResponse>(
            request,
            "/fund-credit-transfer",
            "Fund transfer failed",
            ct
        );
    }

    private async Task<(TResponse? data, string error)> PostXmlAsync<TRequest, TResponse>(
        TRequest request,
        string endpoint,
        string failureMessage,
        CancellationToken cancellationToken
    )
        where TRequest : class
        where TResponse : class
    {
        return await _pipeline.ExecuteAsync(
            async ct =>
            {
                try
                {
                    var xmlRequest = XmlSerializationHelper.Serialize(request);
                    var content = new StringContent(
                        xmlRequest,
                        System.Text.Encoding.UTF8,
                        "application/xml"
                    );
                    var response = await _client.PostAsync(endpoint, content, ct);

                    if (!response.IsSuccessStatusCode)
                    {
                        // log
                        return (null, failureMessage);
                    }

                    var xmlResponse = await response.Content.ReadAsStringAsync(ct);
                    return (XmlSerializationHelper.Deserialize<TResponse>(xmlResponse), "");
                }
                catch (Exception)
                {
                    // TODO: Add retry and structured logging with request details.
                    return (null, "Service unavailable");
                }
            },
            cancellationToken
        );
    }
}
