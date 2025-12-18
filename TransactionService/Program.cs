using Confluent.Kafka;
using FluentValidation;
using KafkaMessages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Polly;
using Scalar.AspNetCore;
using SharedGrpcContracts.Protos.Account.Operations.V1;
using SharedGrpcContracts.Protos.Customers.Notification.Prefrences.V1;
using TransactionService.Data;
using TransactionService.DTOs.NipInterBank;
using TransactionService.NIBBS;
using TransactionService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddControllers();

// fluent validations
builder.Services.AddValidatorsFromAssemblyContaining<NameEnquiryValidator>(
    ServiceLifetime.Singleton
);

// Add and validate connectionString option on startup
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Add dbContext with postgresql
builder.Services.AddDbContext<TransactionDbContext>(options =>
    options.UseNpgsql(
        connectionString,
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(1),
                errorCodesToAdd: null
            );
        }
    )
);

// Mock Nibss Service Http typed client with xml accept header
builder.Services.Configure<NibssOptions>(options =>
{
    options.ApiKey =
        builder.Configuration["NibssSettings:NibssApiKey"]
        ?? throw new InvalidOperationException("Nibss API key is not configured.");
    options.BaseUrl =
        builder.Configuration["NibssSettings:NibssApiUrl"]
        ?? throw new InvalidOperationException("Nibss Base URL is not configured.");
});
builder.Services.AddOptions<NibssOptions>().ValidateDataAnnotations().ValidateOnStart();

builder.Services.AddHttpClient<NibssService>(
    (provider, client) =>
    {
        var nibssOptions = provider.GetRequiredService<IOptions<NibssOptions>>().Value;
        client.BaseAddress = new Uri(nibssOptions.BaseUrl);
        client.DefaultRequestHeaders.Add("api_key", nibssOptions.ApiKey);
        client.DefaultRequestHeaders.Add("Accept", "application/xml");
    }
);

builder.Services.AddResiliencePipeline(
    "key",
    pipelineBuilder =>
    {
        pipelineBuilder.AddRetry(
            new Polly.Retry.RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<HttpRequestException>(ex =>
                        ex.StatusCode >= System.Net.HttpStatusCode.BadRequest
                        && ex.StatusCode <= System.Net.HttpStatusCode.InternalServerError
                    )
                    .Handle<TimeoutException>(),
                BackoffType = DelayBackoffType.Linear,
                MaxRetryAttempts = 3,
                MaxDelay = TimeSpan.FromMilliseconds(100),
                UseJitter = true,
            }
        );
    }
);

// Add gRPC client for Account service
var accountGrpcUrl = builder.Configuration["GrpcSettings:AccountServiceUrl"];
if (string.IsNullOrEmpty(accountGrpcUrl))
    throw new InvalidOperationException("gRPC URL for Account Service is not configured.");

builder
    .Services.AddGrpcClient<AccountOperationsGrpcService.AccountOperationsGrpcServiceClient>(
        options =>
        {
            options.Address = new Uri(accountGrpcUrl);
        }
    )
    .AddStandardResilienceHandler();

var profileGrpcUrl = builder.Configuration["GrpcSettings:AccountServiceUrl"];
if (string.IsNullOrEmpty(profileGrpcUrl))
    throw new InvalidOperationException("gRPC URL for Account Service is not configured.");

builder
    .Services.AddGrpcClient<CustomerNotificationGrpcPrefrenceService.CustomerNotificationGrpcPrefrenceServiceClient>(
        options =>
        {
            options.Address = new Uri(profileGrpcUrl);
        }
    )
    .AddStandardResilienceHandler();

builder.Services.AddScoped<UserPreferenceService>();

builder.Services.AddScoped<NipInterBankService>();

// Kafka Singleton Producer
builder.Services.AddSingleton<IProducer<string, string>>(kp =>
{
    var config = new ProducerConfig
    {
        BootstrapServers = KafkaGlobalConfig.BootstrapServers,
        Acks = Acks.All, // Leader and replica acknowledges writes
        EnableIdempotence = true, // prevents duplicates
        SocketKeepaliveEnable = true,
    };

    var producer = new ProducerBuilder<string, string>(config).Build();

    // dispose on application stopping
    var lifeTime = kp.GetRequiredService<IHostApplicationLifetime>();
    lifeTime.ApplicationStopping.Register(() => producer.Dispose());

    return producer;
});

// Background producer
builder.Services.AddScoped<UserPreferenceService>();
builder.Services.AddHostedService<TransactionNotificationPublisher>();

var app = builder.Build();

app.MapDefaultEndpoints();

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
