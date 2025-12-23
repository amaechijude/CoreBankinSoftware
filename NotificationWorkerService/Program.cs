using System.Net.Sockets;
using MailKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using NotificationWorkerService;
using NotificationWorkerService.Email;
using Polly;

var builder = Host.CreateApplicationBuilder(args);

// email options
builder.Services.Configure<MailKitSettings>(
    builder.Configuration.GetSection(MailKitSettings.Section)
);
builder.Services.AddSingleton<IValidateOptions<MailKitSettings>, EmailOptionsValidator>();
builder.Services.AddOptions<MailKitSettings>().ValidateOnStart();
builder.Services.AddTransient<EmailService>();

// resilience pipeline
// Resilience Pipeline
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

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
