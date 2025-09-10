using YarpApiGateWay;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddYarpConfiguration();


var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapReverseProxy();

app.Run();
