using Microsoft.Extensions.Options;
using Notification.IOptions;
using Notification.Services;

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
