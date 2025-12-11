namespace CustomerAPI.Entities.Enums;

public enum ComplianceCheckStatus
{
    Pending,
    Passed,
    Failed,
    Overdue,
}

public enum ComplianceCheckType
{
    AML,
    PEP,
    Sanctions,
    KYC,
    FraudDetection,
    RiskAssessment,
}
