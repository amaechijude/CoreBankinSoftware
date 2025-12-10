using MailKit.Security;
using Polly;

namespace Notification.Workers;

public static class PollyMailkitHandler
{
    private static IAsyncPolicy HandleSmptpAuth =>
        Policy
            .Handle<AuthenticationException>()
            .CircuitBreakerAsync(0, TimeSpan.FromMilliseconds(1));
}
