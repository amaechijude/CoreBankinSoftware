using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quartz;
using Scalar.AspNetCore;
using SharedGrpcContracts.Protos.Account.V1;
using TransactionService.CustomOptions;
using TransactionService.Data;
using TransactionService.KafaConfig;
using TransactionService.NIBBS;
using TransactionService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
// Add services to the container.
builder.Services.AddControllers();

// Add and validate connectionString option on startup
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

var quartzConnectionString = builder.Configuration.GetConnectionString("QuartzConnection")
    ?? throw new InvalidOperationException("Connection string 'QuartzConnection' not found.");

var hangFireConnectionString = builder.Configuration.GetConnectionString("HanfireConnection")
    ?? throw new InvalidOperationException("Connection string 'HanfireConnection' not found.");

// Add dbcontext with postgreql
builder.Services.AddDbContext<TransactionDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(1),
            errorCodesToAdd: null
        );
    })
);


// Mock Nibss Service Http typed client with xml accept header
builder.Services.Configure<NibssOptions>(options =>
{
    options.ApiKey = builder.Configuration["NibssSettings:NibssApiKey"]
        ?? throw new InvalidOperationException("Nibss API key is not configured.");
    options.BaseUrl = builder.Configuration["NibssSettings:NibssApiUrl"]
        ?? throw new InvalidOperationException("Nibss Base URL is not configured.");
});
builder.Services.AddOptions<NibssOptions>()
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddHttpClient<NibssService>((provider, client) =>
{
    var nibssOptions = provider.GetRequiredService<IOptions<NibssOptions>>().Value;
    client.BaseAddress = new Uri(nibssOptions.BaseUrl);
    client.DefaultRequestHeaders.Add("api_key", nibssOptions.ApiKey);
    client.DefaultRequestHeaders.Add("Accept", "application/xml");
});


// Add gRPC client for Account service
var accountGrpcUrl = builder.Configuration["GrpcSettings:AccountServiceUrl"];
if (string.IsNullOrEmpty(accountGrpcUrl))
    throw new InvalidOperationException("gRPC URL for Account Service is not configured.");
builder.Services.AddGrpcClient<AccountGrpcApiService.AccountGrpcApiServiceClient>(options =>
{
    options.Address = new Uri(accountGrpcUrl);
});

builder.Services.AddScoped<NipInterBankService>();
builder.Services.AddScoped<IntraBankService>();

builder.Services.AddCustomKafkaServiceExtentions();

// Quartz Scheduler for background jobs
// builder.Services.AddQuartz(qz =>
// {
//     qz.UsePersistentStore(options =>
//     {
//         options.UsePostgres(p =>
//         {
//             p.ConnectionString = quartzConnectionString;
//             p.TablePrefix = "quartz_";
//         });
//     });
// });

// builder.Services.AddQuartzHostedService(qz => qz.WaitForJobsToComplete = true);

// // Hangfire
// builder.Services.AddHangfire(config => config
//     .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
//     .UseSimpleAssemblyNameTypeSerializer()
//     .UseRecommendedSerializerSettings()
//     .UsePostgreSqlStorage(hangFireConnectionString)
// );

// builder.Services.AddHangfireServer();
// // builder.Services.AddMvc();

var app = builder.Build();

app.MapDefaultEndpoints();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => "Hello World!");

app.Run();
