using Microsoft.EntityFrameworkCore;
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

// builder.Services.AddScoped<PerformTransaction>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
// app.MapGet("/grpc", async (PerformTransaction performTransaction) =>
// {
//     return await performTransaction.GetAccountDetails("cf8b1da1-c5fd-43f6-9cd8-e80fcaa32ada");
// });

app.Run();
