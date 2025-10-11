namespace TransactionService.Entity;

public class TransactionNibssDetail
{
    public Guid Id { get; set; }
    public string TransactionReference { get; set; } = string.Empty;
    public string? NibbsSessionId { get; set; }
    public string? NibbsResponseCode { get; set; }
    public string? NibbsResponseMessage { get; set; }
    public string? FailureReason { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
