
using TransactionService.NIBBS.XmlQueryAndResponseBody;

namespace TransactionService.NIBBS;

public class NibssService(HttpClient client)
{
    private readonly HttpClient _client = client;

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

    public async Task<(FTSingleDebitResponse? data, string error)> FundTransferDebitAsync(FTSingleDebitRequest request)
    {
        return await PostXmlAsync<FTSingleDebitRequest, FTSingleDebitResponse>(request, "/debit-fund-transfer", "Debit fund transfer failed");
    }
    public async Task<(TSQuerySingleResponse? data, string error)> TransactionStatusQueryAsync(TSQuerySingleRequest request)
    {
        return await PostXmlAsync<TSQuerySingleRequest, TSQuerySingleResponse>(request, "/transaction-status-query", "Transaction status query failed");
    }

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
