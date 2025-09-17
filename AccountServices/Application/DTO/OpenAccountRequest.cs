using AccountServices.Domain.Enums;

namespace AccountServices.Application.DTO
{
  public sealed class OpenAccountRequest
  {
    public Guid CustomerId { get; set; }
    public AccountType Type { get; set; }
    public Currency Currency { get; set; }
    public decimal OpeningBalance { get; set; } = 0m;
  }
}
