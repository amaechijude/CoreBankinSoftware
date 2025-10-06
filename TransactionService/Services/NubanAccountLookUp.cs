using System.ComponentModel.DataAnnotations;

namespace TransactionService.Services;

public sealed class NubanAccountLookUp(HttpClient client)
{
    private readonly HttpClient _client = client;
    public async Task<AccountDetails?> GetAccountDetails(string accountNumber, string bankCode)
    {
        AccountDetails? accountDetails = await _client
            .GetFromJsonAsync<AccountDetails>($"?account_number={accountNumber}&bank_code={bankCode}");
        return accountDetails;
    }
}

public sealed class NubanOptions
{
    [Required, MinLength(10)]
    public string ApiKey { get; set; } = string.Empty;
    [Required, Url, MinLength(10)]
    public string BaseUrl { get; set; } = string.Empty;
}

public sealed class AccountDetails
{
    public string AccountName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string OtherName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
}