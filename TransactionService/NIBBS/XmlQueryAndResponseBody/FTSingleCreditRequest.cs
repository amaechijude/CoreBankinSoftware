using System.Xml.Serialization;

namespace TransactionService.NIBBS.XmlQueryAndResponseBody;

[XmlRoot("FTSingleCreditRequest")]
public class FTSingleCreditRequest
{
    [XmlElement("SessionID")]
    public required string SessionID { get; set; }

    [XmlElement("DestinationBankCode")]
    public required string DestinationBankCode { get; set; }

    [XmlElement("ChannelCode")]
    public required string ChannelCode { get; set; }

    [XmlElement("AccountName")]
    public required string AccountName { get; set; }

    [XmlElement("DestinationAccountNumber")]
    public required string AccountNumber { get; set; }

    [XmlElement("OriginatorName")]
    public required string OriginatorName { get; set; }

    [XmlElement("Narration")]
    public required string Narration { get; set; }

    [XmlElement("PaymentReference")]
    public required string PaymentReference { get; set; }

    [XmlElement("Amount")]
    public required decimal Amount { get; set; }
}


[XmlRoot("FTSingleCreditResponse")]
public class FTSingleCreditResponse
{
    [XmlElement("SessionID")]
    public required string SessionID { get; set; }

    [XmlElement("DestinationBankCode")]
    public required string DestinationBankCode { get; set; }

    [XmlElement("ChannelCode")]
    public required string ChannelCode { get; set; }

    [XmlElement("AccountName")]
    public required string AccountName { get; set; }

    [XmlElement("DestinationAccountNumber")]
    public required string AccountNumber { get; set; }

    [XmlElement("OriginatorName")]
    public required string OriginatorName { get; set; }

    [XmlElement("Narration")]
    public required string Narration { get; set; }

    [XmlElement("PaymentReference")]
    public required string PaymentReference { get; set; }

    [XmlElement("Amount")]
    public required decimal Amount { get; set; }

    [XmlElement("ResponseCode")]
    public required string ResponseCode { get; set; }
}

