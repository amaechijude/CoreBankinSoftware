using System.Xml.Serialization;

namespace TransactionService.NIBBS.XmlQueryAndResponseBody;

[XmlRoot(ElementName = "NESingleRequest")]
public class NESingleRequest
{
    public required string SessionID { get; set; }
    public required string DestinationBankCode { get; set; }
    public required string ChannelCode { get; set; }
    public required string AccountNumber { get; set; }
}


[XmlRoot(ElementName = "NESingleResponse")]
public class NESingleResponse
{
    public required string SessionID { get; set; }
    public required string DestinationBankCode { get; set; }
    public required string ChannelCode { get; set; }
    public required string AccountNumber { get; set; }
    public required string AccountName { get; set; }
    public required string ResponseCode { get; set; }
}