namespace AccountServices.Application.DTO
{
  public sealed class MoneyRequest
  {
    public decimal Amount { get; set; }
    public string Narration { get; set; } = "operation";
  }
}
