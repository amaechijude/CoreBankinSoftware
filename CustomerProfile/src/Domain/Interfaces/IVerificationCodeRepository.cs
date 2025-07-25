using src.Domain.Entities;

namespace src.Domain.Interfaces
{
    public interface IVerificationCodeRepository
    {
        Task<VerificationCode> AddVerificationCodeAsync(VerificationCode verificationCode);
        Task<VerificationCode?> GetVerificationCodeAsync(string phoneNumber, string code);
        Task RemoveVerificationCodeAsync(string phoneNumber, string code);
    }
}
