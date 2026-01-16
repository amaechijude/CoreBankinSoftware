using FluentValidation;

namespace CustomerProfile.DTO.BvnNinVerification;

public sealed record BvnSearchRequest(string Bvn); // exactly 11 digits

public class BvnSearchRequestValidator : AbstractValidator<BvnSearchRequest>
{
    public BvnSearchRequestValidator()
    {
        RuleFor(x => x.Bvn)
            .Length(11)
            .WithMessage("Bvn must be exactly 11 digits")
            .NotEmpty()
            .WithMessage("Bvn Cannot be empty")
            .Must(ContainOnlyDigits)
            .WithMessage("Bvn must contain only digits");
    }

    private bool ContainOnlyDigits(string bvn) =>
        !string.IsNullOrEmpty(bvn) && bvn.All(char.IsAsciiDigit);
}

public sealed record NinSearchRequest(string Nin); // exactly 11 digits

public class NinSearchRequestValidator : AbstractValidator<NinSearchRequest>
{
    public NinSearchRequestValidator()
    {
        RuleFor(x => x.Nin)
            .Length(11)
            .WithMessage("Nin must be exactly 11 digits")
            .NotEmpty()
            .WithMessage("Nin Cannot be empty")
            .Must(nin => !string.IsNullOrEmpty(nin) && nin.All(char.IsAsciiDigit))
            .WithMessage("Nin must contain only digits");
    }
}

public sealed record SearchResponse(string Message, bool Status);

public sealed record FaceVerificationRequest(IFormFile ImageFile);

public sealed class FaceVerificationRequestValidator : AbstractValidator<FaceVerificationRequest>
{
    private const long MaxFileSizeInBytes = 15 * 1024 * 1024; // 15 MB
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly HashSet<string> AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/jpg",
        "image/webp",
    ];

    public FaceVerificationRequestValidator()
    {
        RuleFor(x => x.ImageFile)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("Image file is required.")
            .Must(file => file != null && file.Length > 0)
            .WithMessage("Image file cannot be empty.")
            .Must(file => file != null && file.Length <= MaxFileSizeInBytes)
            .WithMessage(
                $"Image file size must not exceed {MaxFileSizeInBytes / (1024 * 1024)} MB."
            )
            .Must(file => file != null && AllowedContentTypes.Contains(file.ContentType))
            .WithMessage("Only WebP, JPEG and PNG image formats are allowed.")
            .Must(HaveValidExtension)
            .WithMessage("Invalid file extension.")
            .Must(HaveValidSignature)
            .WithMessage(
                "Invalid file content. The file signature does not match the expected image format."
            );
    }

    private static bool HaveValidExtension(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        return !string.IsNullOrEmpty(ext) && AllowedExtensions.Contains(ext);
    }

    private static bool HaveValidSignature(IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            var header = new byte[12];
            var bytesRead = stream.Read(header, 0, header.Length);
            stream.Position = 0; // Reset stream position for subsequent readers

            // JPG: FF D8 FF
            if (bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
                return true;

            // PNG: 89 50 4E 47 0D 0A 1A 0A
            if (
                bytesRead >= 8
                && header[0] == 0x89
                && header[1] == 0x50
                && header[2] == 0x4E
                && header[3] == 0x47
                && header[4] == 0x0D
                && header[5] == 0x0A
                && header[6] == 0x1A
                && header[7] == 0x0A
            )
                return true;

            // WebP: RIFF .... WEBP (RIFF at 0, WEBP at 8)
            if (
                bytesRead >= 12
                && header[0] == 0x52
                && header[1] == 0x49
                && header[2] == 0x46
                && header[3] == 0x46
                && header[8] == 0x57
                && header[9] == 0x45
                && header[10] == 0x42
                && header[11] == 0x50
            )
                return true;

            return false;
        }
        catch
        {
            return false;
        }
    }
}
