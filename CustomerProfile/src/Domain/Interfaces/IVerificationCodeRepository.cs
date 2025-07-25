using src.Domain.Entities;

namespace src.Domain.Interfaces
{
    public interface IVerificationCodeRepository
    {
        Task<VerificationCode> AddAsync(VerificationCode verificationCode);
        Task<VerificationCode?> GetAsync(string phoneNumber);
        Task<VerificationCode?> GetAsync(string phoneNumber, string code);
        Task RemoveAsync(string phoneNumber, string code);
        Task SaveChangesAsync();
    }
}
