using System.Net.Sockets;
using Confluent.Kafka;
using CoreBankingSoftware.ServiceDefaults;
using KafkaMessages;
using MailKit;
using MailKit.Net.Smtp;
using Notification;
using Notification.Email;
using Notification.SMS;
using Polly;

var builder = WebApplication.CreateBuilder(args);

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
//builder.AddKafkaConsumer<string, string>(
//    "kafka",
//    consumer =>
//    {
//        consumer.Config.GroupId = KafkaGlobalConfig.NotificationGroupId;
//        consumer.Config.AutoOffsetReset = AutoOffsetReset.Earliest;
//        consumer.Config.EnableAutoCommit = false;
//    }
//);

//builder.Services.AddSingleton<IConsumer<string, string>>(c =>
//{
//    var config = new ConsumerConfig {
//        BootstrapServers = KafkaGlobalConfig.BootstrapServers,
//        GroupId = KafkaGlobalConfig.NotificationGroupId,
//        AllowAutoCreateTopics = true,
//        AutoOffsetReset = AutoOffsetReset.Earliest,
//        EnableAutoCommit = false, // commit after processeing

//    };
//    var consumer = new ConsumerBuilder<string, string>(config).Build();

//    var lifetime = c.GetRequiredService<IHostApplicationLifetime>();
//    lifetime.ApplicationStopping.Register(() => consumer.Dispose());

//    return consumer;
//});

//builder.Services.AddHostedService<Worker>();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing",
    "Bracing",
    "Chilly",
    "Cool",
    "Mild",
    "Warm",
    "Balmy",
    "Hot",
    "Sweltering",
    "Scorching",
};

app.MapGet(
        "/weatherforecast",
        () =>
        {
            var forecast = Enumerable
                .Range(1, 5)
                .Select(index => new WeatherForecast(
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
                .ToArray();
            return forecast;
        }
    )
    .WithName("GetWeatherForecast");

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
