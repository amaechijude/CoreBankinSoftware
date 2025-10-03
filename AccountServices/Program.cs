using AccountServices.Data;
using AccountServices.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddGrpc();

// Add and validate connectionString option on startup
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Add dbcontext with postgreql
builder.Services.AddDbContext<AccountDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AccountDbContext>();
    SeedData.Initialize(dbContext);
}

app.MapGrpcService<AccountService>();
app.MapGet("/", () => "Account Service is running...");

app.MapControllers();
app.Run();
