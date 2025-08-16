using Scalar.AspNetCore;
using Serilog;
using UserProfile.API;
using UserProfile.API.Shared.Messaging;

string folder = Path.Combine(Directory.GetCurrentDirectory(), "log");
if (!Directory.Exists(folder))
    Directory.CreateDirectory(folder);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File($"{folder}/log-.txt", rollingInterval: RollingInterval.Hour)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add Service Extensions
builder.Services.AddCustomerDbContext();
builder.Services.AddFeaturesServices();
builder.Services.AddQuickVerifyServices();

// Add Messaging Service
builder.Services.AddSMSMessageServices();

builder.Services.AddControllers();
builder.Services.AddCustomerDbContext();

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

app.Run();
