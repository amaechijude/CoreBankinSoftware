using System.Net.Sockets;
using MailKit.Net.Smtp;
using Polly;
using Polly.Retry;

namespace Notification.Workers;

public static class PollyMailkitHandler
{
    public static readonly ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
        .AddRetry(
            new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<TimeoutException>()
                    .Handle<SocketException>()
                    .Handle<HttpRequestException>()
                    .Handle<SmtpProtocolException>(),

                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(2),
                UseJitter = true,
            }
        )
        .Build();

    public const string Pkey = "pipeline";
}
