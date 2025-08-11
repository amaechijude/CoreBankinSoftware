namespace src.Features.AddressOCRServices
{
    public class TesseractOcrService(IHostEnvironment hostEnvironment, ILogger<TesseractOcrService> logger)
    {
        private readonly string _dataPath = Path.Combine(hostEnvironment.ContentRootPath, "tessdata");
        private readonly ILogger<TesseractOcrService> _logger = logger;


        public bool AddressOcrValidation(IFormFile image, string address)
        {
            
            return true;
        }
    }
}