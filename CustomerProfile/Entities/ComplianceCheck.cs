using CustomerProfile.Entities.Enums;

namespace CustomerProfile.Entities;

public sealed class ComplianceCheck : BaseEntity
{
    public Guid UserProfileId { get; set; }
    public UserProfile UserProfile { get; set; } = null!;

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
