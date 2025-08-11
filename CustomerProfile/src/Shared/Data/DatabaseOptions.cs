using System.ComponentModel.DataAnnotations;

namespace src.Shared.Data
{
    public sealed class DatabaseOptions
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
    }
}
