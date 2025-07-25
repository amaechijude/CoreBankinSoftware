using src.Domain.Entities;
using src.Domain.Enums;

namespace src.Domain.Interfaces
{
    public interface ICustomerRepository
    {
        // Savechanges Async
        Task SaveChangesAsync();
        // Add new customer
        Task<Customer> AddAsync(Customer customer);
        Task<Customer?> GetByCustomerNumberAsync(string customerNumber);
        Task<Customer?> GetByBVNAsync(string bvn);
        Task<Customer?> GetByNINAsync(string nin);
        Task<Customer?> GetByPhoneNumberAsync(string phoneNumber);
        Task<Customer?> GetByEmailAsync(string email);
        Task<Customer?> GetCustomerWithFullDetailsAsync(Guid customerId);
        Task<Customer?> GetCustomerWithFullDetailsAsync(string customerNumber);

        Task<IEnumerable<Customer>> GetCustomersByStatusAsync(CustomerStatus status);
        Task<IEnumerable<Customer>> GetCustomersByKYCStatusAsync(KYCStatus kycStatus);
        Task<IEnumerable<Customer>> GetCustomersByAccountTierAsync(AccountTier accountTier);
        Task<IEnumerable<Customer>> GetCustomersByRiskLevelAsync(RiskLevel riskLevel);
        Task<IEnumerable<Customer>> GetCustomersByTypeAsync(CustomerType customerType);

        Task<IEnumerable<Customer>> GetMinorCustomersAsync();
        Task<IEnumerable<Customer>> GetPEPCustomersAsync();
        Task<IEnumerable<Customer>> GetWatchlistedCustomersAsync();
        Task<IEnumerable<Customer>> GetCustomersWithExpiredDocumentsAsync();
        Task<IEnumerable<Customer>> GetCustomersRequiringKYCRenewalAsync();
        Task<IEnumerable<Customer>> GetDormantCustomersAsync(int inactiveDays = 365);

        Task<bool> DoesCustomerNumberExistAsync(string customerNumber);
        Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm);
        Task<Dictionary<CustomerStatus, int>> GetCustomerStatusSummaryAsync();
        Task<Dictionary<KYCStatus, int>> GetKYCStatusSummaryAsync();
    }
}
