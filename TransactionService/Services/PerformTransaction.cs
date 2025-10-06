namespace TransactionService.Services;

public class PerformTransaction(NubanAccountLookUp nubanAccountLookUp)
{
    public async Task<ApiResponse> GetBeneficiaryAccountDetails(string accountNumber, string bankName, string? bankNubanCode)
    {
        string? bankCode = !string.IsNullOrWhiteSpace(bankNubanCode) ? bankNubanCode : BankCodes.GetBankCode(bankName);
        if (bankCode == null)
            return ApiResponse.Error("Bank not supported");

        try
        {
            AccountDetails? accountDetails = await nubanAccountLookUp.GetAccountDetails(accountNumber, bankCode);
            if (accountDetails == null)
                return ApiResponse.Error("Account not found");

            return ApiResponse.Success("Account found", accountDetails);
        }
        catch (Exception)
        {
            // Log the exception (ex) as needed
            return ApiResponse.Error("An error occurred while fetching account details");
        }
    }

    public async Task<ApiResponse> InitiateTransaction()
    {
        try
        {
            // Simulate transaction processing logic
            await Task.Delay(1000); // Simulating async work

            // In a real-world scenario, you would call your transaction processing service here
            // For example:
            // var transactionResult = await transactionService.ProcessTransaction(...);

            // Assuming the transaction was successful
            return ApiResponse.Success("Transaction initiated successfully", new { TransactionId = Guid.NewGuid() });
        }
        catch (Exception)
        {
            // Log the exception (ex) as needed
            return ApiResponse.Error("An error occurred while initiating the transaction");
        }
    }
}

public class ApiResponse
{
    public bool Status { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public object? Data { get; private set; }

    public static ApiResponse Success(string message, object? data = null)
    {
        return new ApiResponse { Status = true, Message = message, Data = data };
    }

    public static ApiResponse Error(string message)
    {
        return new ApiResponse { Status = false, Message = message };
    }

}