using src.Domain.Enums;
using src.Domain.ValueObjects;

namespace src.Domain.Entities
{
    public class Customer : BaseEntity
    {
        public Customer(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("Phone number cannot be null or empty.", nameof(phoneNumber));
            PhoneNumber = phoneNumber;
        }
        // Private fields for collections
        private readonly List<Address> _addresses = [];
        private readonly List<NextOfKin> _nextOfKins = [];
        private readonly List<KYCDocument> _kycDocuments = [];
        private readonly List<ComplianceCheck> _complianceChecks = [];
        private readonly List<RiskAssessment> _riskAssesments  = [];



        // Enums
        public CustomerType CustomerType { get; set; } = CustomerType.Individual;
        public CustomerStatus Status { get; set; } = CustomerStatus.PendingActivation;
        public KYCStatus KYCStatus { get; private set; } = KYCStatus.NotStarted;
        public AccountTier AccountTier { get; set; } = AccountTier.Tier1;
        public EmployementType EmploymentType { get; set; } = EmployementType.SelfEmployed;
        public CustomerSource Source { get; set; } = CustomerSource.WalkIn;


        // Personal Infomartion
        public string CustomerNumber { get; private set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string? MaidenName { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public string PlaceOfBirth { get; set; } = string.Empty;
        public string StateOfOrigin { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? MaritalStatus { get; set; }

        // Contact Information
        public string Email { get; private set; } = string.Empty;
        public string? AlternateEmail { get; private set; }
        public string PhoneNumber { get; private set; } = string.Empty;
        public string? AlternatePhoneNumber { get; private set; }

        // Nigerian Banking Specific Identifiers
        public BVN BVN { get; private set; } = string.Empty; // Bank Verification Number
        public NIN NIN { get; private set; } = string.Empty; // National Identification Number
        public DateTimeOffset? BVNaddedAt { get; private set; }
        public DateTimeOffset? NINAddedAt { get; private set; }

        // Biometrics
        public string? ImageUrl { get; private set; } // URL to the customer's image
        public float[]? FaceEnconding { get; private set; } // Array of floats representing face encoding
        public float[]? FingerprintTemplate { get; private set; } // Array of floats representing fingerprint data template
        public string? SignatureImageUrl { get; private set; } // URL to the customer's signature image
        public DateTimeOffset? LastTransactionDate { get; private set; }

        // Address Information

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
       public bool KycNeedsRenewal
        {
            get
            {
                if (_kycDocuments.Any(k => k.IsExpired))
                    return true;
                return false;
            }
        }

        public RiskLevel RiskLevel
        {
            get
            {
                if (_riskAssesments.Count != 0)
                    return _riskAssesments.Last().RiskLevel;
                return RiskLevel.Low;
            }
        }
 

        // Public Properties for Collections
        public IReadOnlyCollection<Address> Addresses => _addresses.AsReadOnly();
        public IReadOnlyCollection<NextOfKin> NextOfKins => _nextOfKins.AsReadOnly();
        public IReadOnlyCollection<KYCDocument> KYCDocuments => _kycDocuments.AsReadOnly();
        public IReadOnlyCollection<ComplianceCheck> ComplianceChecks => _complianceChecks.AsReadOnly();
        public IReadOnlyCollection<RiskAssessment> RiskAssessments => _riskAssesments.AsReadOnly();


        // Methods to add items to collections
        public void AddAddress(Address address)
        {
            if (_addresses.Count >= 5)
                throw new InvalidOperationException("Cannot add more than 5 addresses.");

            // check address is already added
            if (_addresses.Any(a => a.Equals(address)))
                throw new InvalidOperationException("This address has already been added.");

            _addresses.Add(address);
            LastTransactionDate = DateTimeOffset.UtcNow;
        }
        public void RemoveAddress(Address address)
        {
            if (!_addresses.Remove(address))
                throw new InvalidOperationException("Address not found.");
            _addresses.Remove(address);
            LastTransactionDate = DateTimeOffset.UtcNow;
        }
        public void AddKYCDocument(KYCDocument kycDocument)
        {
            var existingDocument = _kycDocuments.FirstOrDefault(d => d.DocumentType == kycDocument.DocumentType);
            if (existingDocument != null && existingDocument.IsExpired)
                throw new InvalidOperationException("Cannot add a new KYC document of the same type if the existing one is expired.");

            if (kycDocument.IsExpired)
                throw new InvalidOperationException("Cannot add an expired KYC document.");
            // check document is already added
            if (_kycDocuments.Any(d => d.Equals(kycDocument)))
                throw new InvalidOperationException("This KYC document has already been added.");

            _kycDocuments.Add(kycDocument);
            LastTransactionDate = DateTimeOffset.UtcNow;
        }

        public void UpdateKYCDocument(KYCDocument kycDocument)
        {
            var existingDocument = _kycDocuments.FirstOrDefault(d => d.DocumentType == kycDocument.DocumentType)
                ?? throw new InvalidOperationException("KYC document not found.");
            
            existingDocument = kycDocument;
            LastTransactionDate = DateTimeOffset.UtcNow;
        }

        public void AddNextOfKin(NextOfKin nextOfKin)
        {
            if (_nextOfKins.Count >= 5)
                throw new InvalidOperationException("Cannot add more than 5 next of kin.");

            // check next of kin is already added
            if (_nextOfKins.Any(n => n.Equals(nextOfKin)))
                throw new InvalidOperationException("This next of kin has already been added.");

            _nextOfKins.Add(nextOfKin);
            LastTransactionDate = DateTimeOffset.UtcNow;
        }

        public void AddNIN(string nin)
        {
            NIN = new NIN(nin);
        }

        public void AddBVN(string bvn)
        {
            BVN = new BVN(bvn);
        }

    }
}
