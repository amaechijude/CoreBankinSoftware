using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace src.Infrastructure.External.Messaging.SMS
{
    public sealed class SMSBackgroundService : BackgroundService
    {
        private readonly TwilioSmsSender _twilloSmsSender;
        private readonly Channel<SendSMSCommand> _smsChannel;
        private readonly ILogger<SMSBackgroundService> _logger;

        // Retry configuration
        private readonly int _maxRetryAttempts = 3;
        private readonly TimeSpan _baseDelay = TimeSpan.FromSeconds(2);

        public SMSBackgroundService(
            TwilioSmsSender twilioSmsSender,
            Channel<SendSMSCommand> smsChannel,
            ILogger<SMSBackgroundService> logger
            )
        {
            _twilloSmsSender = twilioSmsSender;
            _smsChannel = smsChannel;
            _logger = logger;
        }
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
                    _logger.LogError(ex, "Failed to send SMS.");
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
                    attempt++;
                    if (attempt >= _maxRetryAttempts)
                    {
                        _logger.LogCritical("Failed to send SMS to after {MaxAttempts} attempts.", _maxRetryAttempts);
                        return;
                    }
                    var delay = CalculateExponentialBackoff(attempt);
                    await Task.Delay(delay, cancellationToken);
                }
            }
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
                _ => false
            };
        }

    }
}