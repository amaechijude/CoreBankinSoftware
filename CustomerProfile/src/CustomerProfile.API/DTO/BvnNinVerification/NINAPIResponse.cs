using System.Text.Json.Serialization;

namespace CustomerAPI.DTO.BvnNinVerification
{
    public sealed class NINAPIResponse
    {
        [JsonPropertyName("status")]
        public bool Status { get; set; }

        [JsonPropertyName("data")]
        public NINPersonalData? Data { get; set; }
    }

    public sealed class NINPersonalData
    {
        [JsonPropertyName("birthdate")]
        public string? Birthdate { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("emplymentstatus")]
        public string? EmploymentStatus { get; set; }

        [JsonPropertyName("firstname")]
        public string? FirstName { get; set; }

        [JsonPropertyName("gender")]
        public string? Gender { get; set; }

        [JsonPropertyName("maritalstatus")]
        public string? MaritalStatus { get; set; }

        [JsonPropertyName("middlename")]
        public string? MiddleName { get; set; }

        [JsonPropertyName("nin")]
        public string? Nin { get; set; }

        [JsonPropertyName("photo")]
        public string? Photo { get; set; }

        [JsonPropertyName("profession")]
        public string? Profession { get; set; }

        [JsonPropertyName("religion")]
        public string? Religion { get; set; }

        [JsonPropertyName("residence_AddressLine1")]
        public string? ResidenceAddressLine1 { get; set; }

        [JsonPropertyName("residence_Town")]
        public string? ResidenceTown { get; set; }

        [JsonPropertyName("residence_lga")]
        public string? ResidenceLga { get; set; }

        [JsonPropertyName("residence_state")]
        public string? ResidenceState { get; set; }

        [JsonPropertyName("residencestatus")]
        public string? ResidenceStatus { get; set; }

        [JsonPropertyName("signature")]
        public string? Signature { get; set; }

        [JsonPropertyName("surname")]
        public string? Surname { get; set; }

        [JsonPropertyName("telephoneno")]
        public string? TelephoneNo { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("trackingId")]
        public string? TrackingId { get; set; }
    }
}
