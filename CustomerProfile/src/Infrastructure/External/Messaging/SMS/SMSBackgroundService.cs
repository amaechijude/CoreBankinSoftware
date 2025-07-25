using System.Net.Sockets;
using System.Threading.Channels;

namespace src.Infrastructure.External.Messaging.SMS
{
    public class SMSBackgroundService(
        TwilioSmsSender twilioSmsSender,
        Channel<SendSMSCommand> smsChannel,
        ILogger<SMSBackgroundService> logger
            ) : BackgroundService
    {
        private readonly TwilioSmsSender _twilloSmsSender = twilioSmsSender;
        private readonly Channel<SendSMSCommand> _smsChannel = smsChannel;
        private readonly ILogger<SMSBackgroundService> _logger = logger;

        // Retry configuration
        private readonly int _maxRetryAttempts = 3;
        private readonly TimeSpan _baseDelay = TimeSpan.FromSeconds(2);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var command in _smsChannel.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    await ProcessSmsWithRetryAsync(command, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to send SMS. {ErrorMessage}", ex.Message);
                }
            }
        }

        private async Task ProcessSmsWithRetryAsync(SendSMSCommand command, CancellationToken cancellationToken)
        {
            int attempt = 0;
            while (attempt < _maxRetryAttempts)
            {
                try
                {
                    await _twilloSmsSender.SendAsync(command);
                    return; // Success, exit the loop
                }
                catch (Exception ex) when (IsRetriableException(ex))
                {
                    if (attempt >= _maxRetryAttempts)
                    {
                        _logger.LogCritical("Failed to send SMS to after {MaxAttempts} attempts.", _maxRetryAttempts);
                        return; // Exit after max attempts
                    }
                    attempt++;
                    var delay = CalculateExponentialBackoff(attempt);
                    await Task.Delay(delay, cancellationToken);
                }
            }
            return;
        }

        private TimeSpan CalculateExponentialBackoff(int attempt)
        {
            var delay = TimeSpan.FromTicks(_baseDelay.Ticks * (1L << attempt));

            var jitter = Random.Shared.NextDouble() * 0.3; // ±30% jitter
            var jitterMs = delay.TotalMilliseconds * jitter;

            return delay.Add(TimeSpan.FromMilliseconds(jitterMs));
        }

        private static bool IsRetriableException(Exception ex)
        {
            return ex switch
            {
                HttpRequestException => true,
                TimeoutException => true,
                SocketException => true,
                TaskCanceledException => true,

                _ => false
            };
        }

    }

}