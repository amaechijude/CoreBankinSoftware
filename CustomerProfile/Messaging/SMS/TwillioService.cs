using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace CustomerProfile.Messaging.SMS;

public sealed class TwilioSmsSender(
    IOptions<TwilioSettings> twilioSettings,
    ILogger<TwilioSmsSender> logger
) : ISmsSender
{
    private readonly IOptions<TwilioSettings> _twilioSettings = twilioSettings;
    private readonly ILogger<TwilioSmsSender> _logger = logger;

    public async Task SendAsync(SendSMSCommand command)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(command.PhoneNumber))
                throw new ArgumentException(
                    "Phone number cannot be null or empty.",
                    nameof(command)
                );

            if (string.IsNullOrWhiteSpace(command.Message))
                throw new ArgumentException("Message cannot be null or empty.", nameof(command));

            // Ensure the phone number starts with a '+' sign
            if (command.PhoneNumber[0] != '+')
                command = command with { PhoneNumber = "+" + command.PhoneNumber };

            await SendViaTwilioAsync(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS via Twilio.");
            throw;
        }
    }

    private async Task SendViaTwilioAsync(SendSMSCommand command)
    {
        TwilioClient.Init(_twilioSettings.Value.AccountSid, _twilioSettings.Value.AuthToken);

        var messageOptions = new CreateMessageOptions(
            new Twilio.Types.PhoneNumber(command.PhoneNumber)
        )
        {
            From = new Twilio.Types.PhoneNumber(_twilioSettings.Value.FromPhoneNumber),
            Body = command.Message,
        };

        await MessageResource.CreateAsync(messageOptions);
    }
}

public record SendSMSCommand(string PhoneNumber, string Message);

public interface ISmsSender
{
    Task SendAsync(SendSMSCommand command);
}
