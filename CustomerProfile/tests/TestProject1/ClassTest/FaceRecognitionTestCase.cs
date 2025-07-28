//using Microsoft.Extensions.DependencyInjection;
//using System.Net.Http;
//using src.Features.FaceRecognotion;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.DependencyModel.Resolution;

//namespace TestProject1.ClassTest
//{
//    public class FaceRecognitionTestCase
//    {
//        readonly FaceRecognitionService _faceRecognition;
//        readonly string _testImageUrl;
//        public FaceRecognitionTestCase()
//        {
//            var services = new ServiceCollection();
//            services.AddHttpClient();

//            var httpClientFactory = services
//                .BuildServiceProvider()
//                .GetRequiredService<IHttpClientFactory>();

//            _faceRecognition = new FaceRecognitionService(httpClientFactory);
//            _testImageUrl = "https://x.com/amaechi_1/photo";
//        }

//        // [Fact]
//        // public async Task MatchUserFaceWithNinImageAndGenerateEmbedding_WithValidFileType_ReturnsTrueAndFloatEmbedding()
//        // {
//        //     IFormFile? formFile = CreateFormFile("image.png");

//        //     var (isValid, embedding) = await _faceRecognition.MatchUserFaceWithNinImageAndGenerateEmbedding(formFile, _testImageUrl);
//        //     Assert.True(isValid);
//        //     Assert.NotNull(embedding);
//        // }

//        [Fact]
//        public void TestImageFileExist()
//        {
//            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "testFiles", "image.png");
//            var fileInfo = new FileInfo(filePath);
//            Assert.True(fileInfo.Exists);
//            Assert.Contains(fileInfo.Name, "image.png");
//        }

//        // [Fact]
//        // public async Task MatchUserFaceWithNinImageAndGenerateEmbedding_WithNullFile_ReturnsFalseAndNullEmbedding()
//        // {
//        //     IFormFile? formFile = CreateFormFile("image.png");

//        //     var result = await _faceRecognition.MatchUserFaceWithNinImageAndGenerateEmbedding(formFile,);
//        //     Assert.False(false);
//        // }

//        // [Fact]
//        // public void MatchUserFaceWithNinImageAndGenerateEmbedding_WithInvalidNinImageUrl_ReturnsFalseAndNullEmbedding()
//        // {
//        //     Assert.False(false);
//        // }

//        // [Fact]
//        // public void MatchUserFaceWithNinImageAndGenerateEmbedding_WithInvalidFileType_ReturnsFalseAndNullEmbedding()
//        // {
//        //     Assert.False(false);
//        // }

//        private static FormFile? CreateFormFile(string filename)
//        {
//            try
//            {

//                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "testFiles");
//                if (!Directory.Exists(filePath))
//                    Directory.CreateDirectory(filePath);

//                filePath = Path.Combine(filePath, filename);
//                var fileInfo = new FileInfo(filePath);

//                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
//                return new FormFile(
//                   baseStream: fileStream,
//                   baseStreamOffset: 0,
//                   length: fileInfo.Length,
//                   name: "file",
//                   fileName: fileInfo.Name
//               );

//            }
//            catch { return null; }
//        }

//    }
//}