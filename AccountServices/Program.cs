using AccountServices;
using AccountServices.Data;
using AccountServices.Validators;
using Confluent.Kafka;
using KafkaMessages;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddGrpc();

// Add and validate connectionString option on startup
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AccountDbContext>(options =>
    options.UseNpgsql(
        connectionString,
        npgSqlOptions =>
        {
            npgSqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(0.4),
                errorCodesToAdd: null
            );
        }
    )
);

// kafka producer
builder.Services.AddSingleton(kp =>
{
    var config = new ProducerConfig { BootstrapServers = KafkaGlobalConfig.BootstrapServers };
    var producer = new ProducerBuilder<string, string>(config).Build();
    return producer;
});

// Fluent validator
builder.Services.AddSingleton<CreateAccountRequestValidator>();

// Resilience
builder.Services.AddSingleton<CustomResiliencePolicy>();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
}

// app.MapGrpcService<AccountProtoService>();
app.MapGet("/", () => "Account Service is running...");

app.MapControllers();
app.Run();
