using System.Xml.Serialization;

namespace TransactionService.NIBBS.XmlQueryAndResponseBody;

[XmlRoot("FTSingleDebitRequest")]
public class FTSingleDebitRequest
{
    public required string SessionID { get; set; }
    public required string DestinationBankCode { get; set; }
    public required string ChannelCode { get; set; }
    public required string AccountName { get; set; }
    public required string AccountNumber { get; set; }
    public required string BillerName { get; set; }
    public required string BillerID { get; set; }
    public required string Narration { get; set; }
    public required string PaymentReferenceNumber { get; set; }
    public required string MandateReferenceNumber { get; set; }
    public required decimal Amount { get; set; }
}

[XmlRoot("FTSingleDebitResponse")]
public class FTSingleDebitResponse
{
    public required string SessionID { get; set; }
    public required string DestinationBankCode { get; set; }
    public required string ChannelCode { get; set; }
    public required string AccountName { get; set; }
    public required string AccountNumber { get; set; }
    public required string BillerName { get; set; }
    public required string BillerID { get; set; }
    public required string Narration { get; set; }
    public required string PaymentReferenceNumber { get; set; }
    public required string MandateReferenceNumber { get; set; }
    public required decimal Amount { get; set; }
    public required string ResponseCode { get; set; }
}
