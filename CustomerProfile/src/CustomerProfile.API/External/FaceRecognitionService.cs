using CustomerAPI.DTO;
using FaceAiSharp;
using FaceAiSharp.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CustomerAPI.External
{
   public sealed class FaceRecognitionService(
       ILogger<FaceRecognitionService> logger,
       IFaceDetector faceDetector,
       IFaceEmbeddingsGenerator faceEmbeddingsGenerator)
   {
      private readonly ILogger<FaceRecognitionService> _logger = logger;
      private readonly IFaceDetector _faceDetector = faceDetector;
      private readonly IFaceEmbeddingsGenerator _faceEmbeddingsGenerator = faceEmbeddingsGenerator;

      public async Task<ApiResponse<FaceComparisonResponse>> CompareFaces(IFormFile image1, string base64Image)
      {
         Task<ProcessImageResult> task1 = ProcessLocalImage(image1);
         Task<ProcessImageResult> task2 = ProcessBase64Image(base64Image);

         var result1 = await task1;
         var result2 = await task2;

         if (!result1.IsSuccess)
            return ApiResponse<FaceComparisonResponse>.Error(result1.ErrorMessage);

         if (!result2.IsSuccess)
            return ApiResponse<FaceComparisonResponse>.Error(result2.ErrorMessage);

         var Similarity = result1.Embedding!.Dot(result2.Embedding!);

         var comparison = Similarity switch
         {
            >= 0.42f => "Same Person",
            > 0.28f => "Uncertain - might be same person",
            _ => "Different Person"
         };

         return ApiResponse<FaceComparisonResponse>.Success(
            new FaceComparisonResponse
            {
               Similarity = Similarity >= 0.42,
               Comparison = comparison,
               Image1Embeddings = result1.Embedding,
               Image2Embeddings = result2.Embedding,
               Image1FaceCount = result1.FaceCount,
               Image2FaceCount = result2.FaceCount,
               Image1Confidence = result1.Confidence,
               Image2Confidence = result2.Confidence
            }
         );
      }
      private async Task<ProcessImageResult> ProcessLocalImage(IFormFile image)
      {
         try
         {
            using var stream = image.OpenReadStream();
            using var img = await Image.LoadAsync<Rgb24>(stream);

            var faces = _faceDetector.DetectFaces(img);

            if (faces.Count == 0)
               return ProcessImageResult.Error("No faces detected in the image.");
            if (faces.Count > 1)
               return ProcessImageResult.Error("More than one face detected in the image.");

            var face = faces.First();

            using var alignedImage = img.Clone(); // Clone Image For Alignement
            _faceEmbeddingsGenerator.AlignFaceUsingLandmarks(alignedImage, face.Landmarks!);

            var embedding = _faceEmbeddingsGenerator.GenerateEmbedding(img);

            return ProcessImageResult.Success(embedding, faces.Count, face.Confidence);
         }
         catch (Exception ex)
         {
            _logger.LogError(ex, "Image processing failed.");
            return ProcessImageResult.Error("Image processing failed.");
         }
      }

      private async Task<ProcessImageResult> ProcessBase64Image(string base64String)
      {
         try
         {
            byte[] imageBytes;

            // Check if it's a data URL format (data:image/jpeg;base64,...)
            if (base64String.StartsWith("data:image"))
            {
               // Extract the base64 part after the comma
               var base64Data = base64String[(base64String.IndexOf(',') + 1)..];
               imageBytes = Convert.FromBase64String(base64Data);
            }
            else {imageBytes = Convert.FromBase64String(base64String); }

            using var imageStream = new MemoryStream(imageBytes);
            // Load the image
            using var img = await Image.LoadAsync<Rgb24>(imageStream);
            var faces = _faceDetector.DetectFaces(img);

            if (faces.Count > 1)
               return ProcessImageResult.Error("More than one face detected in the image.");
            if (faces.Count == 0)
               return ProcessImageResult.Error("No faces detected in the image.");

            var face = faces.First();
            using var alignedImage = img.Clone(); // Clone Image For Alignment
            _faceEmbeddingsGenerator.AlignFaceUsingLandmarks(alignedImage, face.Landmarks!);

            var embedding = _faceEmbeddingsGenerator.GenerateEmbedding(img);

            return ProcessImageResult.Success(embedding, faces.Count, face.Confidence);
         }
         catch (FormatException ex)
         {
            _logger.LogError(ex, "Invalid base64 format");
            return ProcessImageResult.Error("Invalid base64 image format.");
         }
         catch (ArgumentException ex)
         {
            _logger.LogError(ex, "Invalid base64 string");
            return ProcessImageResult.Error("Invalid base64 string provided.");
         }
         catch (UnknownImageFormatException ex)
         {
            _logger.LogError(ex, "Unsupported image format in base64 data");
            return ProcessImageResult.Error("Unsupported image format. Please ensure the base64 data represents a valid image file (JPEG, PNG, GIF, BMP, TIFF, WebP, etc.)");
         }
         catch (Exception ex)
         {
            _logger.LogError(ex, "Image processing failed for base64 data");
            return ProcessImageResult.Error("Image processing failed.");
         }
      }
   }

   public class FaceComparisonResponse
   {
      public bool Similarity { get; set; }
      public string Comparison { get; set; } = string.Empty;
      public float[]? Image1Embeddings { get; set; }
      public float[]? Image2Embeddings { get; set; }
      public int Image1FaceCount { get; set; }
      public int Image2FaceCount { get; set; }
      public float? Image1Confidence { get; set; }
      public float? Image2Confidence { get; set; }
   }

   public sealed class ProcessImageResult
   {
      public bool IsSuccess { get; private set; }
      public string ErrorMessage { get; private set; } = string.Empty;
      public float[]? Embedding { get; private set; }
      public int FaceCount { get; private set; }
      public float? Confidence { get; private set; }

      public static ProcessImageResult Error(string message)
      {
         return new ProcessImageResult { IsSuccess = false, ErrorMessage = message };
      }
      public static ProcessImageResult Success(float[]? embedding, int faceCount, float? confidence)
      {
         return new ProcessImageResult
         {
            IsSuccess = true,
            Embedding = embedding,
            FaceCount = faceCount,
            Confidence = confidence
         };
      }
   }
   
}
