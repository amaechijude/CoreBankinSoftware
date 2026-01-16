using System.ComponentModel.DataAnnotations;
using CustomerProfile.Entities.Enums;
using Microsoft.AspNetCore.Http;

namespace CustomerProfile.DTO;

public record UploadKycRequest(
    [Required] IFormFile Document,
    [Required] KycDocumentType DocumentType,
    [Required] string DocumentNumber
);

public record KycDocumentResponse(
    Guid Id,
    string DocumentType,
    string DocumentNumber,
    bool IsVerified,
    string? VerificationReference,
    DateTimeOffset? ExpiryDate
);
