using System.ComponentModel.DataAnnotations;

namespace CustomerProfile.Data;

public sealed record DatabaseOptions
{
    [Required, MinLength(3)]
    public string Name { get; set; } = string.Empty;

    [Required, MinLength(3)]
    public string User { get; set; } = string.Empty;

    [Required, MinLength(3)]
    public string Host { get; set; } = string.Empty;

    [Required, MinLength(3)]
    public string Password { get; set; } = string.Empty;

    [Required, MinLength(3)]
    public string Port { get; set; } = string.Empty;
}
