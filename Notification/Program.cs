using Confluent.Kafka;
using CoreBankingSoftware.ServiceDefaults;
using KafkaMessages;
using MailKit;
using MailKit.Net.Smtp;
using Notification;
using Notification.Email;
using Notification.SMS;
using Polly;
using Serilog;
using System.Net.Sockets;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "CustomerProfile.API")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        "Logs/log-.txt",
        rollingInterval: RollingInterval.Hour,
        fileSizeLimitBytes: 10_485_760, // 10 MB
        rollOnFileSizeLimit: true,
        retainedFileCountLimit: null,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

builder.Services.AddSerilog(); // <-- serilog

builder.AddServiceDefaults();

// email
builder
    .Services.Configure<MailKitSettings>(builder.Configuration.GetSection(MailKitSettings.Section))
    .AddOptions<MailKitSettings>()
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddTransient<EmailService>();

// Add Twilio SMS settings
builder
    .Services.Configure<TwilioSettings>(builder.Configuration.GetSection(TwilioSettings.Section))
    .AddOptions<TwilioSettings>()
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Email Resilience Pipeline
builder.Services.AddResiliencePipeline(
    MailKitSettings.ResiliencePipelineKey,
    pipelineBuilder =>
    {
        pipelineBuilder
            .AddRetry(
                new Polly.Retry.RetryStrategyOptions
                {
                    ShouldHandle = new PredicateBuilder()
                        .Handle<SocketException>()
                        .Handle<ServiceNotConnectedException>()
                        .Handle<HttpRequestException>()
                        .Handle<SmtpProtocolException>()
                        .Handle<SmtpCommandException>(sc =>
                            (int)sc.StatusCode >= 400 && (int)sc.StatusCode < 500
                        ),
                    MaxRetryAttempts = 3,
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    Delay = TimeSpan.FromSeconds(2),
                }
            )
            .AddTimeout(TimeSpan.FromSeconds(30));
    }
);

// Add Kafka consumer via Aspire
builder.AddKafkaConsumer<string, string>(
    "kafka",
    consumer =>
    {
        consumer.Config.GroupId = KafkaGlobalConfig.TransactionNotificationGroupId;
        consumer.Config.AutoOffsetReset = AutoOffsetReset.Earliest;
        consumer.Config.EnableAutoCommit = false;
    }
);

builder.Services.AddHostedService<Worker>();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/health", () => new { Error = "Hello" });

app.Run();
