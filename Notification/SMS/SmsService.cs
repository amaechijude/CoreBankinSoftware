using Microsoft.Extensions.Options;

namespace Notification.SMS;

public sealed class SmsService(IOptions<TwilioSettings> twilioSettings, ILogger<SmsService> logger)
{
    private readonly TwilioSettings _twilioSettings = twilioSettings.Value;
    private readonly ILogger<SmsService> _logger = logger;

    public static async Task SendSmsAsync(SmsRequest request, CancellationToken ct) { }

    private async Task SendSmsViaTwilioAsync(SmsRequest request, CancellationToken ct)
    {
        try
        {
            Twilio.TwilioClient.Init(_twilioSettings.AccountSid, _twilioSettings.AuthToken);

            var messageOptions = new Twilio.Rest.Api.V2010.Account.CreateMessageOptions(
                new Twilio.Types.PhoneNumber(NormalizePhoneNumber(request.PhoneNumber))
            )
            {
                From = new Twilio.Types.PhoneNumber(_twilioSettings.FromNumber),
                Body = request.Message,
            };

            await Twilio.Rest.Api.V2010.Account.MessageResource.CreateAsync(messageOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS via Twilio.");
            throw;
        }
    }

    private static string NormalizePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException(
                "Phone number cannot be null or empty.",
                nameof(phoneNumber)
            );
        // normalize phone number to E.164 format
        return phoneNumber;
    }
}

public record SmsRequest(string PhoneNumber, string Message);
