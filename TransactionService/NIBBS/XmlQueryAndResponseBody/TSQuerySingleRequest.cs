using System.Xml.Serialization;

namespace TransactionService.NIBBS.XmlQueryAndResponseBody;

[XmlRoot("TSQuerySingleRequest")]
public class TSQuerySingleRequest
{
    public required string DestinationBankCode { get; set; }
    public required string ChannelCode { get; set; }
    public required string SessionID { get; set; }
}

[XmlRoot("TSQuerySingleResponse")]
public class TSQuerySingleResponse
{
    public required string DestinationBankCode { get; set; }
    public required string ChannelCode { get; set; }
    public required string SessionID { get; set; }
    public required string ResponseCode { get; set; }
}