using System.ComponentModel.DataAnnotations;

namespace TransactionService.DTOs;

public class NameEnquiryRequest
{
    [Required, StringLength(10)]
    public required string SenderAccountNumber { get; set; }
    [Required, MinLength(3)]
    public required string SenderBankName { get; set; }
    [Required, RegularExpression(@"^\d+$", ErrorMessage = "DestinationBankNubanCode must contain only digits.")]
    public required string SenderBankNubanCode { get; set; }
    [Required, StringLength(10)]
    public required string DestinationAccountNumber { get; set; }
    [Required, MinLength(3)]
    public required string DestinationBankName { get; set; }
    [Required, RegularExpression(@"^\d+$", ErrorMessage = "DestinationBankNubanCode must contain only digits.")]
    public required string DestinationBankNubanCode { get; set; }

}

