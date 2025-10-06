
using TransactionService.NIBBS.XmlQueryAndResponseBody;
using TransactionService.Utils;

namespace TransactionService.NIBBS;

public class NibssService(HttpClient client)
{
    private readonly HttpClient _client = client;

    /// <summary>
    /// Performs a name enquiry using the provided request details.
    /// </summary>
    /// <param name="request">The name enquiry request.</param>
    /// <returns>A tuple containing the name enquiry response on success, or an error message on failure.</returns>
    public async Task<(NESingleResponse? data, string error)> NameEnquiryAsync(NESingleRequest request)
    {
        return await PostXmlAsync<NESingleRequest, NESingleResponse>(request, "/name-enquiry", "Name enquiry failed");
    }

    /// <summary>
    /// Initiates a single credit fund transfer, which moves funds from a sender's account to a beneficiary's account.
    /// </summary>
    /// <param name="request">The fund transfer request details.</param>
    /// <returns>A tuple containing the fund transfer response on success, or an error message on failure.</returns>
    public async Task<(FTSingleCreditResponse? data, string error)> FundTransferCreditAsync(FTSingleCreditRequest request)
    {
        return await PostXmlAsync<FTSingleCreditRequest, FTSingleCreditResponse>(request, "/fund-transfer", "Fund transfer failed");
    }
    /// <summary>
    /// Initiates a single debit fund transfer
    /// </summary>
    /// <param name="request">The fund transfer request details.</param>
    /// <returns>A tuple containing the fund transfer response on success, or an error message on failure.</returns>
    public async Task<(FTSingleDebitResponse? data, string error)> FundTransferDebitAsync(FTSingleDebitRequest request)
    {
        return await PostXmlAsync<FTSingleDebitRequest, FTSingleDebitResponse>(request, "/debit-fund-transfer", "Debit fund transfer failed");
    }
    /// <summary>
    /// Queries the status of a single transaction.
    /// </summary>
    /// <param name="request">The transaction status query request details.</param>
    /// <returns>A tuple containing the transaction status query response on success, or an error message on failure.</returns>

    public async Task<(TSQuerySingleResponse? data, string error)> TransactionStatusQueryAsync(TSQuerySingleRequest request)
    {
        return await PostXmlAsync<TSQuerySingleRequest, TSQuerySingleResponse>(request, "/transaction-status-query", "Transaction status query failed");
    }

    /// <summary>
    /// Performs a balance enquiry using the provided request details.
    /// </summary>
    /// <param name="request">The balance enquiry request.</param>
    /// <returns>A tuple containing the balance enquiry response on success, or an error message on failure.</returns>
    public async Task<(BalanceEnquiryResponse? data, string error)> BalanceEnquiryAsync(BalanceEnquiryRequest request)
    {
        return await PostXmlAsync<BalanceEnquiryRequest, BalanceEnquiryResponse>(request, "/balance-enquiry", "Balance enquiry failed");
    }

    private async Task<(TResponse? data, string error)> PostXmlAsync<TRequest, TResponse>(TRequest request, string endpoint, string failureMessage)
        where TRequest : class
        where TResponse : class
    {
        try
        {
            var xmlRequest = XmlSerializationHelper.Serialize(request);
            var content = new StringContent(xmlRequest, System.Text.Encoding.UTF8, "application/xml");
            var response = await _client.PostAsync(endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                // It might be useful to log the status code and response body here for debugging.
                return (null, failureMessage);
            }

            var xmlResponse = await response.Content.ReadAsStringAsync();
            return (XmlSerializationHelper.Deserialize<TResponse>(xmlResponse), "");
        }
        catch (HttpRequestException)
        {
            // TODO: Add structured logging with request details.
            return (null, "");
        }
        catch (TimeoutException)
        {
            return (null, "Request timeout");
        }
    }

}
