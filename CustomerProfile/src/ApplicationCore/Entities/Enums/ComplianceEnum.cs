namespace UserProfile.API.ApplicationCore.Domain.Entities.Enums
{
    public enum ComplianceCheckStatus
    {
        Pending,
        Passed,
        Failed,
        Overdue
    }
    
    public enum ComplianceCheckType
    {
        AML, // Anti-Money Laundering
        PEP, // Politically Exposed Persons
        Sanctions, // Sanctions Lists
        KYC, // Know Your Customer
        FraudDetection, // Fraud Detection Checks
        RiskAssessment // Risk Assessment Checks
    }
}
