using CustomerProfile.Data;
using CustomerProfile.DTO;
using CustomerProfile.Entities;
using CustomerProfile.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace CustomerProfile.Services;

public sealed class KycService(UserProfileDbContext context, IWebHostEnvironment environment)
{
    private readonly UserProfileDbContext _context = context;
    private readonly IWebHostEnvironment _environment = environment;

    public async Task<ApiResponse<string>> UploadDocumentAsync(
        Guid userId,
        UploadKycRequest request,
        CancellationToken ct
    )
    {
        var user = await _context.UserProfiles.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null)
        {
            return ApiResponse<string>.Error("User not found");
        }

        // Validate File
        if (request.Document.Length == 0)
        {
            return ApiResponse<string>.Error("File is empty");
        }

        // Save File (Local Storage for now)
        var uploadsFolder = Path.Combine(_environment.WebRootPath ?? "wwwroot", "uploads", "kyc");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName = $"{Guid.NewGuid()}_{request.Document.FileName}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await request.Document.CopyToAsync(stream, ct);
        }

        var kycDocument = new KYCDocument
        {
            CustomerId = userId,
            DocumentType = request.DocumentType,
            DocumentNumber = request.DocumentNumber,
            FileUrl = $"/uploads/kyc/{uniqueFileName}",
            IsVerified = false,
            // Assuming MimeType based on extension for simplicity, normally IFormFile.ContentType
            DocumentMimeType = DocumentMimeType.Image, // Defaulting or needed a parser
        };

        // Simple check for mime type mapping
        if (request.Document.ContentType.Contains("pdf"))
            kycDocument.DocumentMimeType = DocumentMimeType.PdF;
        else if (request.Document.ContentType.Contains("png"))
            kycDocument.DocumentMimeType = DocumentMimeType.PdF;

        user.KycDocuments.Add(kycDocument);
        await _context.SaveChangesAsync(ct);

        return ApiResponse<string>.Success("Document uploaded successfully");
    }

    public async Task<ApiResponse<List<KycDocumentResponse>>> GetDocumentsAsync(
        Guid userId,
        CancellationToken ct
    )
    {
        var docs = await _context
            .UserProfiles.Where(u => u.Id == userId)
            .SelectMany(u => u.KycDocuments)
            .Select(k => new KycDocumentResponse(
                k.Id,
                k.DocumentType.ToString(),
                k.DocumentNumber,
                k.IsVerified,
                k.VerificationReference,
                k.ExpiryDate
            ))
            .ToListAsync(ct);

        return ApiResponse<List<KycDocumentResponse>>.Success(docs);
    }
}
