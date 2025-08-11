using Scalar.AspNetCore;
using Serilog;
using src;
using src.Infrastructure.External.Messaging;
using src.Shared.Global;

Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add Service Extensions
string connectionString = StartupValidator.ConnectionString();
builder.Services.AddCustomerDbContext(connectionString);
builder.Services.AddFeaturesServices();

// Add Messaging Service
builder.Services.AddSMSMessageServices();

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
