using System.Threading.Channels;
using CoreBankingSoftware.ServiceDefaults;
using CustomerProfile.Data;
using CustomerProfile.DTO;
using CustomerProfile.Entities;
using CustomerProfile.External;
using CustomerProfile.Global;
using CustomerProfile.JwtTokenService;
using CustomerProfile.Messaging.SMS;
using CustomerProfile.Services;
using FaceAiSharp;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "CustomerProfile.API")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        "Logs/log-.txt",
        rollingInterval: RollingInterval.Hour,
        fileSizeLimitBytes: 10_485_760, // 10 MB
        rollOnFileSizeLimit: true,
        retainedFileCountLimit: null,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

builder.Services.AddSerilog(); // <-- serilog

builder.Services.AddGrpc();

// validations
builder.Services.AddValidation();
builder.Services.AddValidatorsFromAssemblyContaining<OnboardingRequestValidator>(
    ServiceLifetime.Singleton
);

// Add Dboptions
builder
    .Services.Configure<DatabaseOptions>(builder.Configuration.GetSection("DatabaseOptions"))
    .AddOptions<DatabaseOptions>()
    .ValidateDataAnnotations()
    .ValidateOnStart();

// configure db context
builder.Services.AddDbContext<UserProfileDbContext>(
    (provider, options) =>
    {
        var ds = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
        string connString =
            $"Host={ds.Host};Database={ds.Name};Username={ds.User};Password={ds.Password};Port={ds.Port}";

        options.UseNpgsql(
            connString,
            npgSqlOptions =>
            {
                npgSqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null
                );
            }
        );
    }
);

// QuickVerify for Nin and Bvn verification
builder.Services.AddScoped<NinBvnService>();
builder
    .Services.Configure<QuickVerifySettings>(
        builder.Configuration.GetSection("QuickVerifySettings")
    )
    .AddOptions<QuickVerifySettings>()
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Http client
builder.Services.AddHttpClient<QuickVerifyBvnNinService>(
    (provider, client) =>
    {
        var quick = provider.GetRequiredService<IOptions<QuickVerifySettings>>().Value;
        client.BaseAddress = new Uri(quick.BaseUrl);
        client.DefaultRequestHeaders.Add(quick.AuthPrefix, quick.ApiKey);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    }
);

// Face Recognition
builder.Services.AddSingleton<IFaceDetector>(_ =>
    FaceAiSharpBundleFactory.CreateFaceDetectorWithLandmarks()
);
builder.Services.AddSingleton(_ => FaceAiSharpBundleFactory.CreateFaceEmbeddingsGenerator());
builder.Services.AddSingleton<FaceRecognitionService>();

// Messaging Service
builder
    .Services.Configure<TwilioSettings>(builder.Configuration.GetSection("TwilioSettings"))
    .AddOptions<TwilioSettings>()
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<TwilioSmsSender>();

builder.Services.AddSingleton(
    Channel.CreateBounded<SendSMSCommand>(
        new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
        }
    )
);
builder.Services.AddHostedService<SMSBackgroundService>();

// Add Jwt Authentication and Services

builder.Services.AddScoped<IPasswordHasher<UserProfile>, PasswordHasher<UserProfile>>();
builder.Services.AddScoped<OnboardService>();
builder.Services.AddJwtAuthDependencyInjection(builder.Configuration);
builder.Services.AddAuthorization(); // Authorization Service

builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapGrpcService<UserPrefernceProtoServices>();

app.MapControllers();
app.MapGet("/", () => new { Success = true, Date = DateTimeOffset.UtcNow });

app.Run();
