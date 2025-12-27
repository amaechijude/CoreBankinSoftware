using CoreBankingSoftware.ServiceDefaults;
using YarpApiGateWay;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder
    .Services.AddReverseProxy()
    .LoadFromMemory(
        routes: RouteClusterConfiguration.routes,
        clusters: RouteClusterConfiguration.clusters
    )
    .AddServiceDiscoveryDestinationResolver();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGet("/", () => "Hello World!");

app.MapReverseProxy();

app.Run();
