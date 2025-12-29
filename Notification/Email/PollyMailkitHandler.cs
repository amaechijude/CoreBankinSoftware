using MailKit.Net.Smtp;
using Polly;
using Polly.Retry;
using System.Net.Sockets;

namespace Notification.Email;

public static class MailkitHandler
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
}
