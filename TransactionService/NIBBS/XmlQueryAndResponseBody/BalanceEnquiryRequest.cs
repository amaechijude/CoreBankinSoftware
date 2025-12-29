
using System.Xml.Serialization;

namespace TransactionService.NIBBS.XmlQueryAndResponseBody;

[XmlRoot("BalanceEnquiryRequest")]
public class BalanceEnquiryRequest
{
    public required string SessionID { get; set; }
    public required string DestinationBankCode { get; set; }
    public required string ChannelCode { get; set; }
    public required string SpecialCode { get; set; }
    public required string AccountName { get; set; }
    public required string AccountNumber { get; set; }
}

[XmlRoot("BalanceEnquiryResponse")]
public class BalanceEnquiryResponse
{
    public required string SessionID { get; set; }
    public required string DestinationBankCode { get; set; }
    public required string ChannelCode { get; set; }
    public required string SpecialCode { get; set; }
    public required string AccountName { get; set; }
    public required string AccountNumber { get; set; }
    public decimal AvailableBalance { get; set; }
    public required string ResponseCode { get; set; }
}