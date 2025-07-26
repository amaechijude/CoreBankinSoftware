using Scalar.AspNetCore;
using Serilog;
using src.Features;
using src.Infrastructure.Extensions;
using src.Infrastructure.External;
using src.Shared.Global;


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Warning()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog();

// Add services to the container.

var connectionString = StartupValidator.ConnectionString();

builder.Services.AddCustomerDatabaseInfra(connectionString);
builder.Services.AddCustomerRepository();
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
