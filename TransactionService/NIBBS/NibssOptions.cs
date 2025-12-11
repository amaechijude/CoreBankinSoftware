using System.ComponentModel.DataAnnotations;

namespace TransactionService.NIBBS;

// nibss Options
public sealed class NibssOptions
{
    [Required, MinLength(10)]
    public string ApiKey { get; set; } = string.Empty;

    [Required, Url, MinLength(10)]
    public string BaseUrl { get; set; } = string.Empty;
}
