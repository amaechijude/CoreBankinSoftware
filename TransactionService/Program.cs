using System.Threading.Channels;
using Confluent.Kafka;
using CoreBankingSoftware.ServiceDefaults;
using FluentValidation;
using KafkaMessages.AccountMessages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using Polly;
using Scalar.AspNetCore;
using SharedGrpcContracts.Protos.Account.Operations.V1;
using SharedGrpcContracts.Protos.Customers.Notification.Prefrences.V1;
using TransactionService.Data;
using TransactionService.DTOs.NipInterBank;
using TransactionService.Entity;
using TransactionService.Entity.Enums;
using TransactionService.NIBBS;
using TransactionService.Services;
using TransactionService.Workers;

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
builder.Services.AddDbContextFactory<TransactionDbContext>(options =>
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

// Add Redis and Hybrid Cache
// builder.Services.AddStackExchangeRedisCache(options =>
// {
//     options.Configuration =
//         builder.Configuration.GetConnectionString("RedisConnection")
//         ?? throw new InvalidOperationException("Redis connection string is not configured.");
// });

builder.AddRedisClient("redis"); // uses Aspire.StackExchange.Redis
builder.Services.AddHybridCache(options =>
{
    options.MaximumKeyLength = 200;
    options.MaximumPayloadBytes = 1024 * 1024 * 300; // 300MB
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(20),
        LocalCacheExpiration = TimeSpan.FromMinutes(17),
    };
});

// Mock Nibss Service Http typed client with xml accept header
builder
    .Services.Configure<NibssOptions>(builder.Configuration.GetSection("NibbsSettings"))
    .AddOptions<NibssOptions>()
    .ValidateDataAnnotations()
    .ValidateOnStart();

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

// Grpc clients with aspire
builder.Services.AddGrpcClient<AccountOperationsGrpcService.AccountOperationsGrpcServiceClient>(
    options =>
    {
        options.Address = new Uri("https://accountservices");
    }
);

builder
    .Services.AddGrpcClient<CustomerNotificationGrpcPrefrenceService.CustomerNotificationGrpcPrefrenceServiceClient>(
        options =>
        {
            options.Address = new Uri("https://customerprofile");
        }
    )
    .AddStandardResilienceHandler();

// Application services
builder.Services.AddScoped<UserPreferenceService>();
builder.Services.AddScoped<NipInterBankService>();
builder.Services.AddScoped<IntraBankService>();

builder.AddKafkaProducer<string, string>(
    "kafka",
    producer =>
    {
        producer.Config.Acks = Acks.All;
        producer.Config.EnableIdempotence = true; // prevents duplicates
        producer.Config.AllowAutoCreateTopics = true;
    }
); // kafka with aspire

// Background Event producers
builder.Services.AddSingleton(
    Channel.CreateBounded<OutboxMessage>(
        new BoundedChannelOptions(1_000) { FullMode = BoundedChannelFullMode.Wait }
    )
);

builder.Services.AddHostedService<NotificationWithChannelsWorker>();
builder.Services.AddHostedService<NotificationWithOutboxWorker>();

// test
builder.Services.AddScoped<TestProduce>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => "Hello World!");
app.MapPost(
    "/test",
    async (TestProduce produce, CancellationToken ct) =>
    {
        try
        {
            var request = new FundCreditTransferRequest(
                IsIntraBank: false,
                IdempotencyKey: Guid.NewGuid().ToString(),
                CustomerId: Guid.NewGuid(),
                SenderAccountNumber: "0123456789",
                SenderBankName: "Our Bank",
                SenderBankNubanCode: "000001",
                SenderAccountName: "Test Sender",
                DestinationAccountNumber: "9876543210",
                DestinationBankName: "Beneficiary Bank",
                DestinationBankNubanCode: "000002",
                DestinationAccountName: "Beneficiary Name",
                Amount: 1000.50m,
                Narration: "Mock transfer for testing",
                DeviceInfo: "UnitTestDevice",
                IpAddress: "127.0.0.1",
                Longitude: null,
                Latitude: null,
                TransactionChannel: "UnitTest"
            );

            var tr = TransactionData.Create(
                request: request,
                transactionType: TransactionType.Credit,
                reference: "sdxhhujoioiui87754311160908886656fcjkllllgdsssaaa",
                category: TransactionCategory.INTRA_BANK_TRANSFER,
                sessionId: "serrionidggdfssyulljjllpo0987543224899999"
            );

            var message = OutboxMessage.Create(tr);

            var @event = new TransactionAccountEvent
            {
                Email = "preference@gmail.com",
                PhoneNumber = "09876543210",
                TransactionId = message.TransactionId,
                TransactionReference = message.TransactionReference,
                SessionId = message.SessionId,
                DestinationAccountNumber = message.DestinationAccountNumber ?? string.Empty,
                DestinationBankName = message.DestinationBankName ?? string.Empty,
                DestinationAccountName = "Amaechi",
                Amount = message.Amount,
                TransactionFee = message.TransactionFee,
                Timestamp = message.CreatedAt,
                EventType = EventType.Deposit,
                SendersAccountName = "Sender Amaechi",
                SendersBankName = message.BankName,
                SendersAccountNumber = "9112708565",
            };

            var p = await produce.ProduceMessageAsync(@event, ct);

            return Results.Ok(
                new
                {
                    Success = p,
                    Date = DateTimeOffset.UtcNow,
                    Message = "Message Published",
                }
            );
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message);
        }
    }
);

app.Run();
