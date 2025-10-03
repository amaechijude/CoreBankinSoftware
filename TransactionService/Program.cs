using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedGrpcContracts.Protos.Account.V1;
using TransactionService.Services;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

// Add and validate connectionString option on startup
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Add dbcontext with postgreql
builder.Services.AddDbContext<TransactionService.Data.TransactionDbContext>(options =>
    options.UseNpgsql(connectionString));

var accountGrpcUrl = builder.Configuration["GrpcSettings:AccountServiceUrl"];
if (string.IsNullOrEmpty(accountGrpcUrl))
    throw new InvalidOperationException("gRPC URL for Account Service is not configured.");

//builder.Services.AddScoped<PerformTransaction>();

// Nuban Service options and Http typed client
builder.Services.Configure<NubanOptions>(options =>
{
    options.ApiKey = builder.Configuration["NubanSettings:NubanApiKey"]
        ?? throw new InvalidOperationException("Nuban API key is not configured.");
    options.BaseUrl = builder.Configuration["NubanSettings:NubanApiUrl"]
        ?? throw new InvalidOperationException("Nuban Base URL is not configured.");
});
builder.Services.AddOptions<NubanOptions>()
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddHttpClient<NubanAccountLookUp>((provider, client) =>
{
    var nubanOptions = provider.GetRequiredService<IOptions<NubanOptions>>().Value;
    client.BaseAddress = new Uri(nubanOptions.BaseUrl);
    client.DefaultRequestHeaders.Add("api_key", nubanOptions.ApiKey);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
