using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using SharedGrpcContracts.Protos.Account.V1;
using TransactionService.NIBBS;
using TransactionService.Services;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllers();

// Add and validate connectionString option on startup
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Add dbcontext with postgreql
builder.Services.AddDbContext<TransactionService.Data.TransactionDbContext>(options =>
    options.UseNpgsql(connectionString));

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

// Nibss Service Http typed client with xml accept header
builder.Services.Configure<NibssOptions>(options =>
{
    options.ApiKey = builder.Configuration["NibssSettings:NibssApiKey"]
        ?? throw new InvalidOperationException("Nibss API key is not configured.");
    options.BaseUrl = builder.Configuration["NibssSettings:NibssApiUrl"]
        ?? throw new InvalidOperationException("Nibss Base URL is not configured.");
});
builder.Services.AddOptions<NibssOptions>()
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddHttpClient<NibssService>((provider, client) =>
{
    var nibssOptions = provider.GetRequiredService<IOptions<NibssOptions>>().Value;
    client.BaseAddress = new Uri(nibssOptions.BaseUrl);
    client.DefaultRequestHeaders.Add("api_key", nibssOptions.ApiKey);
    client.DefaultRequestHeaders.Add("Accept", "application/xml");
});

// Add gRPC client for Account service

var accountGrpcUrl = builder.Configuration["GrpcSettings:AccountServiceUrl"];
if (string.IsNullOrEmpty(accountGrpcUrl))
    throw new InvalidOperationException("gRPC URL for Account Service is not configured.");
builder.Services.AddGrpcClient<AccountGrpcApiService.AccountGrpcApiServiceClient>(options =>
{
    options.Address = new Uri(accountGrpcUrl);

});

builder.Services.AddScoped<PerformTransaction>();

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => "Hello World!");

app.Run();
