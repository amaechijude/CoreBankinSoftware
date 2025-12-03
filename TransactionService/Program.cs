using Confluent.Kafka;
using KafkaMessages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using SharedGrpcContracts.Protos.Account.V1;
using TransactionService.Data;
using TransactionService.NIBBS;
using TransactionService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
// Add services to the container.
builder.Services.AddControllers();

// Add and validate connectionString option on startup
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Add dbContext with postgresql
builder.Services.AddDbContext<TransactionDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(1),
            errorCodesToAdd: null
        );
    })
);


// Mock Nibss Service Http typed client with xml accept header
builder.Services.Configure<NibssOptions>(options =>
{
    options.ApiKey = builder.Configuration["NibssSettings:NibssApiKey"]
        ?? throw new InvalidOperationException("Nibss API key is not configured.");
    options.BaseUrl = builder.Configuration["NibssSettings:NibssApiUrl"]
        ?? throw new InvalidOperationException("Nibss Base URL is not configured.");
});
builder.Services.AddOptions<NibssOptions>()
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddHttpClient<NibssService>((provider, client) =>
{
    var nibssOptions = provider.GetRequiredService<IOptions<NibssOptions>>().Value;
    client.BaseAddress = new Uri(nibssOptions.BaseUrl);
    client.DefaultRequestHeaders.Add("api_key", nibssOptions.ApiKey);
    client.DefaultRequestHeaders.Add("Accept", "application/xml");
});


// Add gRPC client for Account service
var accountGrpcUrl = builder.Configuration["GrpcSettings:AccountServiceUrl"];
if (string.IsNullOrEmpty(accountGrpcUrl))
    throw new InvalidOperationException("gRPC URL for Account Service is not configured.");
builder.Services.AddGrpcClient<AccountGrpcApiService.AccountGrpcApiServiceClient>(options =>
{
    options.Address = new Uri(accountGrpcUrl);
});

builder.Services.AddScoped<NipInterBankService>();
builder.Services.AddScoped<IntraBankService>();

// Kafka Singleton Producer
builder.Services.AddSingleton<IProducer<string, string>>(kp =>
{
    var config = new ProducerConfig
    {
        BootstrapServers = KafkaGlobalConfig.BootstrapServers,
        Acks = Acks.All, // Leader and replica acknowledges writes
        EnableIdempotence = true, // prevents duplicates
        SocketKeepaliveEnable = true
    };

    var producer = new ProducerBuilder<string, string>(config).Build();

    // dispose on application stopping
    var lifeTime = kp.GetRequiredService<IHostApplicationLifetime>();
    lifeTime.ApplicationStopping.Register(() => producer.Dispose());

    return producer;
});

// Background producer
builder.Services.AddHostedService<EventPublisher>();

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
