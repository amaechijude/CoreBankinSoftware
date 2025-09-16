using CustomerAPI;
using CustomerAPI.Global;
using CustomerAPI.JwtTokenService;
using CustomerAPI.Messaging;
using CustomerAPI.Services.AccountAPI;
using Scalar.AspNetCore;
using Serilog;


var builder = WebApplication.CreateBuilder(args);


Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "CustomerProfile.API")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .MinimumLevel.Information()
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

builder.Services.AddAccountApiOptions(); // Account API Options

// Add Jwt Authentication
builder.Services.AddJwtAuthDependencyInjection();
builder.Services.AddAuthorization(); // Authorization Service

builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.UseExceptionHandler();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
