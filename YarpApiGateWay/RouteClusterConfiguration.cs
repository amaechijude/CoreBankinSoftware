using Yarp.ReverseProxy.Configuration;

namespace YarpApiGateWay;

internal static class RouteClusterConfiguration
{
    const string _customerServiceClusterId = "customerServiceCluster";
    const string _transactionClusterId = "transactionCluster";

    // Router configuration
    internal static readonly RouteConfig[] routes =
    [
        // customer profile
        new RouteConfig()
        {
            RouteId = "customerServiceRoute",
            ClusterId = _customerServiceClusterId,
            Match = new RouteMatch() { Path = "/customer/{**catch-all}" },
        },
        // transaction
        new RouteConfig()
        {
            RouteId = "transaction",
            ClusterId = _transactionClusterId,
            Match = new RouteMatch() { Path = "/transaction/{**catch-all}" },
        },
    ];

    // Cluster configuration
    internal static readonly ClusterConfig[] clusters =
    [
        new ClusterConfig()
        {
            ClusterId = _customerServiceClusterId,
            Destinations = new Dictionary<string, DestinationConfig>(
                StringComparer.OrdinalIgnoreCase
            )
            {
                {
                    "customerServiceDestination",
                    new DestinationConfig() { Address = "https+http://customerprofile" }
                },
            },
        },
        new ClusterConfig()
        {
            ClusterId = _transactionClusterId,
            Destinations = new Dictionary<string, DestinationConfig>(
                StringComparer.OrdinalIgnoreCase
            )
            {
                {
                    "transactionServiceDestination",
                    new DestinationConfig() { Address = "https+http://transactionservice" }
                },
            },
        },
    ];
}
