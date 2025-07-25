using src.Domain.Enums;

namespace src.Domain.Entities
{
    public class KYCDocument : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public Customer? Customer { get; set; }

        //  Document Details
        public DocumentType DocumentType { get; set; }
        public DocumentMimeType DocumentMimeType { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public DateTimeOffset? IssuedAt { get; set; }
        public DateTimeOffset? ExpiryDate { get; set; }
        public bool IsVerified { get; set; } = false;
        public DateTimeOffset? VerifiedAt { get; set; }
        public string? VerifiedBy { get; set; } // User or system that verified the document
        public string? VerificationReference { get; set; } // Reference ID for the verification process
        public string FileUrl { get; set; } = string.Empty;

        // Helper Propeties
        public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTimeOffset.UtcNow;
        public int? DaysUntilExpiry => ExpiryDate.HasValue
            ? (int?)(ExpiryDate.Value - DateTimeOffset.UtcNow).TotalDays
            : null;
    }
}
