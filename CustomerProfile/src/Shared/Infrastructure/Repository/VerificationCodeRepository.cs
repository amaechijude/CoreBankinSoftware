using Microsoft.EntityFrameworkCore;
using src.Shared.Domain.Entities;
using src.Shared.Domain.Interfaces;
using src.Shared.Infrastructure.Data;

namespace src.Shared.Infrastructure.Repository
{
    public class VerificationCodeRepository(CustomerDbContext context) : IVerificationCodeRepository
    {
        private readonly DbSet<VerificationCode> _dbSet = context.Set<VerificationCode>();

        public async Task<VerificationCode> AddVerificationCodeAsync(VerificationCode verificationCode)
        {
            await _dbSet.AddAsync(verificationCode);
            return verificationCode;
        }

        public async Task<VerificationCode?> GetVerificationCodeAsync(string phoneNumber, string code)
        {
            return await _dbSet
                .FirstOrDefaultAsync(vc => vc.UserPhoneNumber == phoneNumber && vc.Code == code);
        }

        public async Task RemoveVerificationCodeAsync(string phoneNumber, string code)
        {
            await _dbSet
                .Where(vc => vc.UserPhoneNumber == phoneNumber && vc.Code == code)
                .ExecuteDeleteAsync();
        }
    }
}
