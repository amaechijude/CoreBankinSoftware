namespace CustomerAPI.Services
{
    public class TesseractOcrService(IHostEnvironment hostEnvironment, ILogger<TesseractOcrService> logger)
    {
        private readonly string _dataPath = Path.Combine(hostEnvironment.ContentRootPath, "tessdata");
        private readonly ILogger<TesseractOcrService> _logger = logger;


        public bool AddressOcrValidation(IFormFile image, string address)
        {
            if (image is null || string.IsNullOrEmpty(address)) 
                return false;
            return true;
        }
    }
}