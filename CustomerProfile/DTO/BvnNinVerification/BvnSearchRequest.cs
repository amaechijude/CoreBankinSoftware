using FluentValidation;

namespace CustomerProfile.DTO.BvnNinVerification
{
    public sealed record BvnSearchRequest(string Bvn); // exactly 11 digits

    public sealed class BvnSearchRequestValidator : AbstractValidator<BvnSearchRequest>
    {
        public BvnSearchRequestValidator()
        {
            RuleFor(x => x.Bvn.Trim())
                .Length(11).WithMessage("Bvn must be exactly 11 digits")
                .NotEmpty().WithMessage("Bvn Cannot be empty")
                .Must(ContainOnlyDigits).WithMessage("Bvn must contain only digits");
        }

        private bool ContainOnlyDigits(string bvn) => bvn.All(char.IsDigit);
    }

    public sealed record FaceVerificationRequest(IFormFile ImageFile);

    public sealed class FaceVerificationRequestValidator : AbstractValidator<FaceVerificationRequest>
    {
        private const long MaxFileSizeInBytes = 15 * 1024 * 1024; // 15 MB
        private static readonly HashSet<string> AllowedContentTypes =
        [
            "image/jpeg",
            "image/png",
            "image/jpg",
            "image/webp"
        ];
        public FaceVerificationRequestValidator()
        {
            RuleFor(x => x.ImageFile)
                .NotNull()
                    .WithMessage("Image file is required.")
                .Must(file => file != null && file.Length > 0)
                    .WithMessage("Image file cannot be empty.")
                .Must(file => file != null && file.Length <= MaxFileSizeInBytes)
                    .WithMessage($"Image file size must not exceed {MaxFileSizeInBytes / (1024 * 1024)} MB.")
                .Must(file => file != null && AllowedContentTypes.Contains(file.ContentType))
                    .WithMessage("Only WebP, JPEG and PNG image formats are allowed.");
        }
    }
}
