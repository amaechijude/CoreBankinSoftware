namespace CustomerAPI.Entities.Enums
{
    public enum CustomerType
    {
        Individual,
        SME,
        Corporate,
        NonProfit,
        Government,
    }
    public enum Gender
    {
        Male,
        Female,
        Other,
    }
    public enum CustomerStatus
    {
        Active,
        Inactive,
        Dormant,
        Suspended,
        Terminated,
        PendingActivation,
        PendingClosure,

        Closed,
    }

    public enum AccountTier
    {
        Tier1,
        Tier2,
        Tier3
    }
    public enum CustomerSource
    {
        Referral,
        Online,
        WalkIn,
        Event,
        Other,
    }
    
    public enum KYCStatus
    {
        NotStarted,
        InProgress,
        Completed,
        Rejected,
        Expired,
        UnderReview,
    }

    public enum KycDocumentType
    {
        Passport,
        NationalID,
        DriverLicense,
        UtilityBill,
        BankStatement,
        EmploymentLetter,
        TaxCertificate,
        BirthCertificate,
        MarriageCertificate,
        DivorceCertificate,
        Other,
    }

    public enum DocumentMimeType
    {
        PDF,
        Image
    }

    public enum DocumentStatus
    {
        Pending,
        Uploaded,
        UnderReview,
        Verified,
        Rejected,
        Expired,
    }

    public enum AddressType
    {
        Residential,
        Business,
        Mailing,
        Permanent
    }

    public enum VerificationMethod
    {
        Manual,
        Automated,
        ThirdParty,
        SelfService,
    }
    public enum ContactMethod
    {
        Email,
        SMS,
    }

}