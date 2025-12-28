using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerProfile.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    NotificationChannels = table.Column<int[]>(type: "integer[]", nullable: false),
                    CustomerType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    KYCStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AccountTier = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EmploymentType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Source = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    UserAccountNumber = table.Column<string>(type: "character(10)", fixedLength: true, maxLength: 10, nullable: true),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MiddleName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MaidenName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    Gender = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    PlaceOfBirth = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StateOfOrigin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Nationality = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Title = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    MaritalStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Username = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Email = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    AlternateEmail = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: false),
                    AlternatePhoneNumber = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: true),
                    BvnHash = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    NinHash = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    BVNAddedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NINAddedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    NinBase64Image = table.Column<string>(type: "text", nullable: true),
                    BvnBase64Image = table.Column<string>(type: "text", nullable: true),
                    FaceEncodings = table.Column<float[]>(type: "real[]", nullable: true),
                    LastTransactionDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsPoliticallyExposedPerson = table.Column<bool>(type: "boolean", nullable: false),
                    PoliticallyExposedPersonSince = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsWatchlisted = table.Column<bool>(type: "boolean", nullable: false),
                    WatchlistScreenedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    WatchlistScreenedUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastAMLScreening = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VerificationCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    UserPhoneNumber = table.Column<string>(type: "text", nullable: false),
                    UserEmail = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    CanSetProfile = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificationCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    AddressType = table.Column<int>(type: "integer", nullable: false),
                    BuildingNumber = table.Column<string>(type: "text", nullable: false),
                    Street = table.Column<string>(type: "text", nullable: false),
                    Landmark = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false),
                    LocalGovernmentArea = table.Column<string>(type: "text", nullable: false),
                    State = table.Column<string>(type: "text", nullable: false),
                    Country = table.Column<string>(type: "text", nullable: false),
                    PostalCode = table.Column<string>(type: "text", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VerifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    VerifiedBy = table.Column<string>(type: "text", nullable: true),
                    VerificationMethod = table.Column<int>(type: "integer", nullable: false),
                    VerificationReference = table.Column<string>(type: "text", nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Addresses_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BvnData",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Gender = table.Column<string>(type: "text", nullable: false),
                    MaritalStatus = table.Column<string>(type: "text", nullable: false),
                    WatchListed = table.Column<string>(type: "text", nullable: false),
                    LevelOfAccount = table.Column<string>(type: "text", nullable: false),
                    BVN = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    MiddleName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    PhoneNumber1 = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber2 = table.Column<string>(type: "text", nullable: false),
                    RegistrationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EnrollmentBank = table.Column<string>(type: "text", nullable: false),
                    EnrollmentBranch = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    LgaOfOrigin = table.Column<string>(type: "text", nullable: false),
                    LgaOfResidence = table.Column<string>(type: "text", nullable: false),
                    Nin = table.Column<string>(type: "text", nullable: false),
                    NameOnCard = table.Column<string>(type: "text", nullable: false),
                    Nationality = table.Column<string>(type: "text", nullable: false),
                    ResidentialAddress = table.Column<string>(type: "text", nullable: false),
                    StateOfOrigin = table.Column<string>(type: "text", nullable: false),
                    StateOfResidence = table.Column<string>(type: "text", nullable: false),
                    Base64Image = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BvnData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BvnData_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComplianceChecks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Details = table.Column<string>(type: "text", nullable: true),
                    CheckedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CheckedBy = table.Column<string>(type: "text", nullable: true),
                    VerificationReference = table.Column<string>(type: "text", nullable: true),
                    NextCheckDue = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceChecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplianceChecks_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KYCDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DocumentMimeType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DocumentNumber = table.Column<string>(type: "text", nullable: false),
                    IssuedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExpiryDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VerifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    VerifiedBy = table.Column<string>(type: "text", nullable: true),
                    VerificationReference = table.Column<string>(type: "text", nullable: true),
                    FileUrl = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KYCDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KYCDocuments_UserProfiles_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NextOfKins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    MiddleName = table.Column<string>(type: "text", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    Gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Nationality = table.Column<string>(type: "text", nullable: false),
                    Occupation = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    AlternatePhoneNumber = table.Column<string>(type: "text", nullable: false),
                    CanContactInEmergency = table.Column<bool>(type: "boolean", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    Relationship = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NextOfKins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NextOfKins_UserProfiles_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NinData",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Birthdate = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    EmploymentStatus = table.Column<string>(type: "text", nullable: true),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    Gender = table.Column<string>(type: "text", nullable: true),
                    MaritalStatus = table.Column<string>(type: "text", nullable: true),
                    MiddleName = table.Column<string>(type: "text", nullable: true),
                    Nin = table.Column<string>(type: "text", nullable: true),
                    Photo = table.Column<string>(type: "text", nullable: true),
                    Profession = table.Column<string>(type: "text", nullable: true),
                    Religion = table.Column<string>(type: "text", nullable: true),
                    ResidenceAddressLine1 = table.Column<string>(type: "text", nullable: true),
                    ResidenceTown = table.Column<string>(type: "text", nullable: true),
                    ResidenceLga = table.Column<string>(type: "text", nullable: true),
                    ResidenceState = table.Column<string>(type: "text", nullable: true),
                    ResidenceStatus = table.Column<string>(type: "text", nullable: true),
                    Signature = table.Column<string>(type: "text", nullable: true),
                    Surname = table.Column<string>(type: "text", nullable: true),
                    TelephoneNo = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    TrackingId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NinData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NinData_UserProfiles_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RiskAssessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    RiskAssessmentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RiskAssessmentSummary = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RiskLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AssessmentDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Assessor = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiskAssessments_UserProfiles_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_UserProfileId",
                table: "Addresses",
                column: "UserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_BvnData_Id",
                table: "BvnData",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BvnData_UserProfileId",
                table: "BvnData",
                column: "UserProfileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceChecks_UserProfileId",
                table: "ComplianceChecks",
                column: "UserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_KYCDocuments_CustomerId",
                table: "KYCDocuments",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_NextOfKins_CustomerId",
                table: "NextOfKins",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_NinData_CustomerId",
                table: "NinData",
                column: "CustomerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NinData_Id",
                table: "NinData",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RiskAssessments_CustomerId",
                table: "RiskAssessments",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_PhoneNumber",
                table: "UserProfiles",
                column: "PhoneNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VerificationCodes_UserPhoneNumber",
                table: "VerificationCodes",
                column: "UserPhoneNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "BvnData");

            migrationBuilder.DropTable(
                name: "ComplianceChecks");

            migrationBuilder.DropTable(
                name: "KYCDocuments");

            migrationBuilder.DropTable(
                name: "NextOfKins");

            migrationBuilder.DropTable(
                name: "NinData");

            migrationBuilder.DropTable(
                name: "RiskAssessments");

            migrationBuilder.DropTable(
                name: "VerificationCodes");

            migrationBuilder.DropTable(
                name: "UserProfiles");
        }
    }
}
