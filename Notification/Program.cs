using System.Net.Sockets;
using MailKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using Notification.IOptions;
using Notification.Services;
using Notification.Workers;
using Polly;
using Polly.Fallback;

var builder = WebApplication.CreateSlimBuilder(args);

builder.AddServiceDefaults();

builder.Services.Configure<EmailOptions>(options =>
{
    var section = builder.Configuration.GetSection(EmailOptions.Section);

    options.FromEmail =
        section["FromEmail"]
        ?? throw new EmailOptionsException($"{nameof(options.FromEmail)} is required");
    options.FromName =
        section["FromName"]
        ?? throw new EmailOptionsException($"{nameof(options.FromName)} is required");
    options.Password =
        section["Password"]
        ?? throw new EmailOptionsException($"{nameof(options.Password)} is required");
    options.SmtpHost =
        section["SmtpHost"]
        ?? throw new EmailOptionsException($"{nameof(options.SmtpHost)} is required");
    options.Username =
        section["Username"]
        ?? throw new EmailOptionsException($"{nameof(options.Username)} is required");

    options.TimeoutSeconds = 30;
    options.UseSsl = builder.Environment.IsProduction();

#pragma warning disable CS8604 // Possible null reference argument.
    options.SmtpPort = int.Parse(section["SmtpPort"]);
#pragma warning restore CS8604 // Possible null reference argument.
});

builder.Services.AddOptions<EmailOptions>().ValidateOnStart();
builder.Services.AddSingleton<IValidateOptions<EmailOptions>, EmailOptionsValidator>();

// Resilience Pipeline
builder.Services.AddResiliencePipeline(
    PollyMailkitHandler.Pkey,
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

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHostedService<NotificationBackgroundProcessor>();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();
