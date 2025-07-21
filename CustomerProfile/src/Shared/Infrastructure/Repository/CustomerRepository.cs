using Microsoft.EntityFrameworkCore;
using src.Shared.Domain.Entities;
using src.Shared.Domain.Enums;
using src.Shared.Domain.Interfaces;
using src.Shared.Infrastructure.Data;

namespace src.Shared.Infrastructure.Repository
{
    public class CustomerRepository(CustomerDbContext context) : ICustomerRepository
    {
        private readonly DbSet<Customer> _dbSet = context.Set<Customer>();

        public async Task<Customer> AddAsync(Customer customer)
        {
            await _dbSet.AddAsync(customer);
            return customer;
        }
        public async Task<Customer?> GetByBVNAsync(string bvn)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.BVN == bvn);
        }

        public async Task<Customer?> GetByCustomerNumberAsync(string customerNumber)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.CustomerNumber == customerNumber);
        }

        public async Task<Customer?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<Customer?> GetByNINAsync(string nin)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.NIN == nin);
        }

        public async Task<Customer?> GetByPhoneNumberAsync(string phoneNumber)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);
        }

        public async Task<IEnumerable<Customer>> GetCustomersByAccountTierAsync(AccountTier accountTier)
        {
            return await _dbSet.Where(c => c.AccountTier == accountTier).ToListAsync();
        }

        public async Task<IEnumerable<Customer>> GetCustomersByKYCStatusAsync(KYCStatus kycStatus)
        {
            return await _dbSet.Where(c => c.KYCStatus == kycStatus).ToListAsync();
        }

        public async Task<IEnumerable<Customer>> GetCustomersByRiskLevelAsync(RiskLevel riskLevel)
        {
            return await _dbSet.Where(c => c.RiskLevel == riskLevel).ToListAsync();
        }

        public async Task<IEnumerable<Customer>> GetCustomersByStatusAsync(CustomerStatus status)
        {
            return await _dbSet.Where(c => c.Status == status).ToListAsync();
        }

        public async Task<IEnumerable<Customer>> GetCustomersByTypeAsync(CustomerType customerType)
        {
            return await _dbSet.Where(c => c.CustomerType == customerType).ToListAsync();
        }

        public async Task<IEnumerable<Customer>> GetCustomersRequiringKYCRenewalAsync()
        {
            return await _dbSet
                .Where(c => c.KYCStatus == KYCStatus.Expired || c.KycNeedsRenewal == true)
                .ToListAsync();  
        }

        public async Task<Dictionary<CustomerStatus, int>> GetCustomerStatusSummaryAsync()
        {
            return await _dbSet
                .GroupBy(c => c.Status)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public async Task<IEnumerable<Customer>> GetCustomersWithExpiredDocumentsAsync()
        {
            return await _dbSet
                .Where(c => c.KYCDocuments.Any(d => d.IsExpired == true))
                .ToListAsync();
        }

        public async Task<Customer?> GetCustomerWithFullDetailsAsync(Guid customerId)
        {
            return await _dbSet
                .Include(c => c.Addresses)
                .Include(c => c.NextOfKins)
                .Include(c => c.KYCDocuments)
                .Include(c => c.ComplianceChecks)
                .Include(c => c.RiskAssessments)
                .FirstOrDefaultAsync(c => c.Id == customerId);
        }

        public async Task<Customer?> GetCustomerWithFullDetailsAsync(string customerNumber)
        {
            return await _dbSet
                .Include(c => c.Addresses)
                .Include(c => c.NextOfKins)
                .Include(c => c.KYCDocuments)
                .Include(c => c.RiskAssessments)
                .FirstOrDefaultAsync(c => c.CustomerNumber == customerNumber);
        }

        public async Task<IEnumerable<Customer>> GetDormantCustomersAsync(int inactiveDays = 365)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(c => c.IsDormant)
                .ToListAsync();
        }

        public async Task<Dictionary<KYCStatus, int>> GetKYCStatusSummaryAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .GroupBy(c => c.KYCStatus)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public async Task<IEnumerable<Customer>> GetMinorCustomersAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Where(c => c.IsMinor)
                .ToListAsync();
        }

        public async Task<IEnumerable<Customer>> GetPEPCustomersAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Where(c => c.IsPoliticallyExposedPerson)
                .ToListAsync();
        }

        public async Task<IEnumerable<Customer>> GetWatchlistedCustomersAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Where(c => c.IsWatchlisted)
                .ToListAsync();
        }

        public async Task<bool> DoesCustomerNumberExistAsync(string customerNumber)
        {
            return await _dbSet.AnyAsync(c => c.CustomerNumber == customerNumber);
        }

        public async Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(c =>
                    c.FirstName.Contains(searchTerm) ||
                    c.LastName.Contains(searchTerm) ||
                    c.CustomerNumber.Contains(searchTerm) ||
                    c.Email.Contains(searchTerm) ||
                    c.PhoneNumber.Contains(searchTerm)
                )
                .ToListAsync();
        }
    }
}