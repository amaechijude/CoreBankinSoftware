namespace KafkaMessages.AccountMessages;

public sealed record CreateAccountEventKafka
{
    public required string AccountName { get; init; }
    public required string AccountNumber { get; init; }
    public required string BankName { get; init; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
}
