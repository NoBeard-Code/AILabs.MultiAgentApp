namespace AILabs.MultiAgentApp.Gateway;

using Yarp.ReverseProxy.Configuration;
using Microsoft.Extensions.Primitives;

public class InMemoryProxyConfigProvider : IProxyConfigProvider
{
    private volatile InMemoryConfig _config;
    private readonly CancellationTokenSource _cts = new();

    public InMemoryProxyConfigProvider()
    {
        _config = new InMemoryConfig([],[]);
    }

    public IProxyConfig GetConfig() => _config;

    public void AddOrUpdateDestination(string clusterId, string destination)
    {
        // Build or update ClusterConfig and RouteConfig
        var cluster = new ClusterConfig
        {
            ClusterId = clusterId,
            Destinations = new Dictionary<string, DestinationConfig>
            {
                [Guid.NewGuid().ToString()] = new DestinationConfig { Address = destination }
            }
        };

        var route = new RouteConfig
        {
            RouteId = $"{clusterId}-route",
            ClusterId = clusterId,
            Match = new RouteMatch { Path = $"/{clusterId}/{{**catchall}}" } // adjust path template
        };

        // Create new snapshot merging existing clusters/routes and replacing clusterId
        var clusters = _config.Clusters.Where(c => c.ClusterId != clusterId).ToList();
        clusters.Add(cluster);
        var routes = _config.Routes.Where(r => r.RouteId != route.RouteId).ToList();
        routes.Add(route);

        _config = new InMemoryConfig(routes.ToArray(), clusters.ToArray());
        // notify YARP to reload
        _cts.Cancel();
    }

    public void RemoveDestination(string clusterId, string destination)
    {
        // Remove cluster and route for simplicity; refine to remove single destination if needed
        var clusters = _config.Clusters.Where(c => c.ClusterId != clusterId).ToArray();
        var routes = _config.Routes.Where(r => r.ClusterId != clusterId).ToArray();
        _config = new InMemoryConfig(routes, clusters);
        _cts.Cancel();
    }

    // Helper class implementing IProxyConfig
    private class InMemoryConfig : IProxyConfig
    {
        public InMemoryConfig(RouteConfig[] routes, ClusterConfig[] clusters)
        {
            Routes = routes;
            Clusters = clusters;
            ChangeToken = new CancellationChangeToken(new CancellationToken(false));
        }
        public IReadOnlyList<RouteConfig> Routes { get; }
        public IReadOnlyList<ClusterConfig> Clusters { get; }
        public IChangeToken ChangeToken { get; }
    }
}
