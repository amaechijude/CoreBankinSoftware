using CustomerAPI;
using CustomerAPI.JwtTokenService;
using CustomerAPI.Messaging;
using Scalar.AspNetCore;
using Serilog;


var builder = WebApplication.CreateBuilder(args);


Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "CustomerProfile.API")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt",
    rollingInterval: RollingInterval.Hour,
    fileSizeLimitBytes: 10_485_760, // 10 MB
    rollOnFileSizeLimit: true,
    retainedFileCountLimit: null,
    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();
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
