var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis").WithDataVolume();

var kafka = builder.AddKafka("kafka").WithKafkaUI(ui => ui.WithHostPort(port: 9094));

var accountservices = builder.AddProject<Projects.AccountServices>("accountservices");
var customerprofile = builder
    .AddProject<Projects.CustomerProfile>("customerprofile")
    .WithReference(accountservices)
    .WithReference(kafka)
    .WaitFor(accountservices);



builder.AddProject<Projects.Notification>("notification")
    .WithReference(kafka)
    .WaitFor(kafka);

var transactionservice = builder
    .AddProject<Projects.TransactionService>("transactionservice")
    .WithReference(accountservices)
    .WithReference(customerprofile)
    .WithReference(redis)
    .WithReference(kafka)
    .WaitFor(accountservices)
    .WaitFor(customerprofile)
    .WaitFor(redis)
    .WaitFor(kafka);

builder
    .AddProject<Projects.YarpApiGateWay>("yarpapigateway")
    .WithReference(transactionservice)
    .WithReference(customerprofile)
    .WithReference(redis)
    // Ensure the gateway waits for backend services to be ready before starting
    // This avoids 502s and proxying to unavailable targets.
    .WaitFor(transactionservice);

builder.Build().Run();
