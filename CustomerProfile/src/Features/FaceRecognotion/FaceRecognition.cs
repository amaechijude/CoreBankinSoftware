using FaceAiSharp;
using FaceAiSharp.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace src.Features.FaceRecognotion
{
    public sealed class FaceRecognition(IHttpClientFactory httpClientFactory)
    {
        public async Task<(bool IsValid, float[]? Embedding)> MatchUserFaceWithNinImageAndGenerateEmbedding(
            IFormFile file,
            string ninImageUrl,
            CancellationToken cancellationToken = default)
        {
            if (file is null || string.IsNullOrWhiteSpace(ninImageUrl))
                return (false, null);

            Task<float[]?> ninImageEmbeddingTask = DownloadNinImageAndGenerateEmbedding(ninImageUrl, httpClientFactory, cancellationToken);
            Task<float[]?> userFaceEmbeddingTask = DetectUserFaceAndGenerateEmbedding(file);

            await Task.WhenAll(ninImageEmbeddingTask, userFaceEmbeddingTask);
            float[]? ninImageEmbedding = ninImageEmbeddingTask.Result;
            float[]? userFaceEmbedding = userFaceEmbeddingTask.Result;

            if (ninImageEmbedding is null || userFaceEmbedding is null)
                return (false, null);

            var dot = ninImageEmbedding.Dot(userFaceEmbedding);

            if (dot < 0.4)
                return (false, null);

            return (true, userFaceEmbedding);

        }
        private static async Task<float[]?> DownloadNinImageAndGenerateEmbedding(
            string ninImageUrl,
            IHttpClientFactory httpClientFactory,
            CancellationToken cancellationToken)
        {
            using var hc = httpClientFactory.CreateClient();
            var imageBytes = await hc.GetByteArrayAsync(ninImageUrl, cancellationToken);
            if (imageBytes.Length == 0)
                return null;

            var image = Image.Load<Rgb24>(imageBytes);
            return GenerateFaceEmbedings(image);
        }
        
        private static async Task<float[]?> DetectUserFaceAndGenerateEmbedding(IFormFile? file)
        {
            if (file is null || file.Length == 0)
                return null;

            // ensure the file is a valid image
            var validImageTypes = new[] { "image/jpeg", "image/png", "image/jpg", "image/webp" };
            if (!validImageTypes.Contains(file.ContentType))
                return null;

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var image = Image.Load<Rgb24>(memoryStream.ToArray());

            var det = FaceAiSharpBundleFactory.CreateFaceDetectorWithLandmarks();
            var faces = det.DetectFaces(image);

            if (faces.Count == 0 || faces.Count > 1)
                return null;

            return GenerateFaceEmbedings(image);
        }

        private static float[] GenerateFaceEmbedings(Image<Rgb24> image)
        {
            var rec = FaceAiSharpBundleFactory.CreateFaceEmbeddingsGenerator();
            
            return rec.GenerateEmbedding(image);
        }
    }
}
