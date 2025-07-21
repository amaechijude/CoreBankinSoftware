using src.Shared.Domain.Enums;

namespace src.Shared.Domain.Entities
{
    public class ComplianceCheck : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        public ComplianceCheckType CheckType { get; set; } // AML, PEP, Sanctions, etc.
        public ComplianceCheckStatus Status { get; set; } = ComplianceCheckStatus.Pending; // Pass, Fail, Pending
        public string? Details { get; set; }
        public DateTimeOffset CheckedAt { get; set; }
        public string? CheckedBy { get; set; }
        public string? VerificationReference { get; set; }
        public DateTimeOffset? NextCheckDue { get; set; }


        // Helper Properties
        public bool IsPassed => Status == ComplianceCheckStatus.Passed;
        public bool IsOverdue => NextCheckDue.HasValue && NextCheckDue.Value < DateTimeOffset.UtcNow;
    }
}
