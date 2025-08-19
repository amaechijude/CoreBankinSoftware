using CustomerAPI;
using CustomerAPI.JwtTokenService;
using CustomerAPI.Messaging;
using Scalar.AspNetCore;
using Serilog;

// serilog configuration

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Hour)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSerilog(); // <-- serilog


// Add Service Extensions
builder.Services.AddCustomServiceExtentions();
builder.Services.AddSMSMessageServices(); // Messaging Service

// Add Jwt Authentication
builder.Services.AddJwtAuthDependencyInjection();
builder.Services.AddAuthorization(); // Authorization Service

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
