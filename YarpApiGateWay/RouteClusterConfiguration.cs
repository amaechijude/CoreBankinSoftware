using Yarp.ReverseProxy.Configuration;

namespace YarpApiGateWay
{
    internal static class RouteClusterConfiguration
    {
        private readonly static string _customerServiceClusterId = "customerServiceCluster";

        // Router configuration
        private static readonly RouteConfig[] routes = [
            new RouteConfig()
            {
                    RouteId = "customerServiceRoute",
                    ClusterId = _customerServiceClusterId,
                    Match = new RouteMatch() { Path = "/customer/{**catch-all}" }
            }
        ];

        // Cluster configuration
        private static readonly ClusterConfig[] clusters = [
            new ClusterConfig()
            {
                ClusterId = _customerServiceClusterId,
                Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "customerServiceDestination", new DestinationConfig() { Address = "http://localhost:5039" } }
                    }
            }
        ];


        // dependency injection
        public static IServiceCollection AddYarpConfiguration(this IServiceCollection services)
        {

            services.AddReverseProxy()
                .LoadFromMemory(routes, clusters);
            return services;
        }
    }
}
