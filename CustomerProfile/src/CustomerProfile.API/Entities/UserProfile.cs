using System.Collections;
using CustomerAPI.DTO.BvnNinVerification;
using CustomerAPI.Entities.Enums;
using CustomerAPI.Entities.ValueObjects;

namespace CustomerAPI.Entities
{
    public class UserProfile : BaseEntity
    {

        // Private fields for collections
        public ICollection<Address> Addresses { get; private set; } = [];
        public ICollection<NextOfKin> NextOfKins { get; private set; } = [];
        public ICollection<KYCDocument> KycDocuments { get; private set; } = [];
        public ICollection<ComplianceCheck> ComplianceChecks { get; private set; } = [];
        public ICollection<RiskAssessment> RiskAssessments { get; private set; } = [];


        // Enums
        public CustomerType CustomerType { get; set; } = CustomerType.Individual;
        public CustomerStatus Status { get; set; } = CustomerStatus.PendingActivation;
        public KYCStatus KYCStatus { get; private set; } = KYCStatus.NotStarted;
        public AccountTier AccountTier { get; set; } = AccountTier.Tier1;
        public EmployementType EmploymentType { get; set; } = EmployementType.SelfEmployed;
        public CustomerSource Source { get; set; } = CustomerSource.Online;


        // Personal Information
        public string? UserAccountNumber { get; private set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MiddleName { get; set; }
        public string? MaidenName { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public string? PlaceOfBirth { get; set; }
        public string? StateOfOrigin { get; set; }
        public string? Nationality { get; set; }
        public string? Title { get; set; }
        public string? MaritalStatus { get; set; }

        // Contact Information
        public string PasswordHash { get; set; } = string.Empty;
        public string Username { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string? AlternateEmail { get; private set; }
        public required string PhoneNumber { get; set; }
        public string? AlternatePhoneNumber { get; private set; }

        // Nigerian Banking Specific Identifiers
        public string BVN { get; private set; } = string.Empty;// Bank Verification Number
        public string NIN { get; private set; } = string.Empty; // National Identification Number
        public BvnData? BvnData { get; private set; }
        public NinData? NinData { get; private set; }
        public DateTimeOffset? BVNAddedAt { get; private set; }
        public DateTimeOffset? NINAddedAt { get; private set; }

        // Biometrics
        public string? ImageUrl { get; private set; } // URL to the customer's image
        public string? NinBase64Image { get; private set; }
        public string? NinBase64Signature { get; private set; }
        public string? BvnBase64Image { get; private set; }
        public float[]? FaceEncodings { get; private set; } // Array of floats representing face encoding
        public DateTimeOffset? LastTransactionDate { get; private set; }

        // Compliance and Risk
        public bool IsPoliticallyExposedPerson { get; private set; } = false;
        public DateTimeOffset? PoliticallyExposedPersonSince { get; set; }
        public bool IsWatchlisted { get; set; } = false;
        public DateTimeOffset? WatchlistScreenedAt { get; set; }
        public DateTimeOffset? WatchlistScreenedUntil { get; set; }
        public DateTimeOffset? LastAMLScreening { get; set; }

        // Helper Properties
        public string FullName => $"{FirstName} {MiddleName} {LastName}";
        public int Age => DateTime.Now.Year - DateOfBirth.Year - (DateTime.Now.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
        public bool IsMinor => Age < 18;
        public bool IsDormant => LastTransactionDate.HasValue && LastTransactionDate.Value < DateTimeOffset.Now.AddDays(-365);
        

        // Factory methods to create user
        public static UserProfile CreateNewUser(string phoneNumber, string email, string username)
        {
            return new UserProfile { PhoneNumber = phoneNumber, Email = email, Username = username };
        }

        // Methods to update sensitive information
        public void AddBvn(BvnApiResponse response)
        {
            if (BvnData is not null || !string.IsNullOrWhiteSpace(BVN))
                return; // BVN already added, do nothing

            var bvnData = BvnData.Create(this, response);
            if (bvnData is null) return;
            
            BvnData = bvnData;
            BvnBase64Image = response.Data?.Base64Image;
            BVNAddedAt = DateTimeOffset.UtcNow;
            return;
        }

        public void RemoveBvn()
        {
            if (BvnData is null && string.IsNullOrWhiteSpace(BVN))
                return; // BVN not set, do nothing
            BvnData = null;
            BVN = string.Empty;
            BvnBase64Image = null;
            BVNAddedAt = null;
            return;
        }

        public bool BvnExists => BvnData is not null && !string.IsNullOrWhiteSpace(BvnBase64Image);

        // Methods to add items to collections
        //public void AddAddress(Address address)
        //{
        //    if (_addresses.Count >= 5)
        //        throw new InvalidOperationException("Cannot add more than 5 addresses.");

        //    // check address is already added
        //    if (_addresses.Any(a => a.Equals(address)))
        //        throw new InvalidOperationException("This address has already been added.");

        //    _addresses.Add(address);
        //    LastTransactionDate = DateTimeOffset.UtcNow;
        //}
        //public void RemoveAddress(Address address)
        //{
        //    if (!_addresses.Remove(address))
        //        throw new InvalidOperationException("Address not found.");
        //    _addresses.Remove(address);
        //    LastTransactionDate = DateTimeOffset.UtcNow;
        //}
        //public void AddKYCDocument(KYCDocument kycDocument)
        //{
        //    var existingDocument = _kycDocuments.FirstOrDefault(d => d.DocumentType == kycDocument.DocumentType);
        //    if (existingDocument != null && existingDocument.IsExpired)
        //        throw new InvalidOperationException("Cannot add a new KYC document of the same type if the existing one is expired.");

        //    if (kycDocument.IsExpired)
        //        throw new InvalidOperationException("Cannot add an expired KYC document.");
        //    // check document is already added
        //    if (_kycDocuments.Any(d => d.Equals(kycDocument)))
        //        throw new InvalidOperationException("This KYC document has already been added.");

        //    _kycDocuments.Add(kycDocument);
        //    LastTransactionDate = DateTimeOffset.UtcNow;
        //}

        //public void UpdateKYCDocument(KYCDocument kycDocument)
        //{
        //    var existingDocument = _kycDocuments.FirstOrDefault(d => d.DocumentType == kycDocument.DocumentType)
        //        ?? throw new InvalidOperationException("KYC document not found.");

        //    existingDocument = kycDocument;
        //    LastTransactionDate = DateTimeOffset.UtcNow;
        //}

        //public void AddNextOfKin(NextOfKin nextOfKin)
        //{
        //    if (_nextOfKins.Count >= 5)
        //        throw new InvalidOperationException("Cannot add more than 5 next of kin.");

        //    // check next of kin is already added
        //    if (_nextOfKins.Any(n => n.Equals(nextOfKin)))
        //        throw new InvalidOperationException("This next of kin has already been added.");

        //    _nextOfKins.Add(nextOfKin);
        //    LastTransactionDate = DateTimeOffset.UtcNow;
        //}

        ////public bool KycNeedsRenewal
        //{
        //    get
        //    {
        //        if (_kycDocuments.Any(k => k.IsExpired))
        //            return true;
        //        return false;
        //    }
        //}

        //public RiskLevel RiskLevel
        //{
        //    get
        //    {
        //        if (_riskAssessments.Count != 0)
        //            return _riskAssessments.Last().RiskLevel;
        //        return RiskLevel.Low;
        //    }
        //}
    }
}
