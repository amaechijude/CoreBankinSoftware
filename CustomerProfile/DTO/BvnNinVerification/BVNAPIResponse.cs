using System.Text.Json.Serialization;

namespace CustomerAPI.DTO.BvnNinVerification
{

    public sealed class BvnApiResponse
    {
        [JsonPropertyName("status")]
        public bool Status { get; set; }

        [JsonPropertyName("detail")]
        public string? Detail { get; set; }

        [JsonPropertyName("response_code")]
        public string? ResponseCode { get; set; }

        [JsonPropertyName("data")]
        public PersonData? Data { get; set; }

        [JsonPropertyName("verification")]
        public VerificationData? Verification { get; set; }
    }

    public sealed class PersonData
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("gender")]
        public string? Gender { get; set; }

        [JsonPropertyName("maritalStatus")]
        public string? MaritalStatus { get; set; }

        [JsonPropertyName("watchListed")]
        public string? WatchListed { get; set; }

        [JsonPropertyName("level0fAccount")]
        public string? LevelOfAccount { get; set; }

        [JsonPropertyName("bvn")]
        public string? Bvn { get; set; }

        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }

        [JsonPropertyName("middleName")]
        public string? MiddleName { get; set; }

        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }

        [JsonPropertyName("dateOfBirth")]
        public DateOnly? DateOfBirth { get; set; }

        [JsonPropertyName("phoneNumber1")]
        public string? PhoneNumber1 { get; set; }

        [JsonPropertyName("phoneNumber2")]
        public string? PhoneNumber2 { get; set; }

        [JsonPropertyName("registrationDate")]
        public DateOnly? RegistrationDate { get; set; }

        [JsonPropertyName("enrollmentBank")]
        public string? EnrollmentBank { get; set; }

        [JsonPropertyName("enrollmentBranch")]
        public string? EnrollmentBranch { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("lgaOfOrigin")]
        public string? LgaOfOrigin { get; set; }

        [JsonPropertyName("lgaOfResidence")]
        public string? LgaOfResidence { get; set; }

        [JsonPropertyName("nin")]
        public string? Nin { get; set; }

        [JsonPropertyName("nameOnCard")]
        public string? NameOnCard { get; set; }

        [JsonPropertyName("nationality")]
        public string? Nationality { get; set; }

        [JsonPropertyName("residentialAddress")]
        public string? ResidentialAddress { get; set; }

        [JsonPropertyName("stateOfOrigin")]
        public string? StateOfOrigin { get; set; }

        [JsonPropertyName("stateOfResidence")]
        public string? StateOfResidence { get; set; }

        [JsonPropertyName("base64Image")]
        public string? Base64Image { get; set; }
    }

    public sealed class VerificationData
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("reference")]
        public string? Reference { get; set; }
    }

}