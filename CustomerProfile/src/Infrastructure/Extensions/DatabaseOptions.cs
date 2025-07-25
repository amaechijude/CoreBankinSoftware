using System.ComponentModel.DataAnnotations;

namespace src.Infrastructure.Extensions
{
    public class DatabaseOptions
    {
        [Required, MinLength(3)]
        public string DatabaseName { get; set; } = string.Empty;
        [Required, MinLength(3)]
        public string DatabaseUser { get; set; } = string.Empty;
        [Required, MinLength(3)]
        public string DatabaseHost { get; set; } = string.Empty;
        [Required, MinLength(3)]
        public string DatabasePassword { get; set; } = string.Empty;
        [Required, MinLength(3)]
        public string DatabasePort { get; set; } = string.Empty;
        public string ConnectionString => $"Server={DatabaseHost};Database={DatabaseName};User Id={DatabaseUser};Password={DatabasePassword};Port={DatabasePort};";
    }
}
