// using Aspire.Hosting;

using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis");
var kafka = builder.AddKafka("kafka").WithKafkaUI();

var accountservices = builder.AddProject<Projects.AccountServices>("accountservices");
var customerprofile = builder
    .AddProject<Projects.CustomerProfile>("customerprofile")
    .WithReference(accountservices)
    .WithReference(kafka);

builder
    .AddProject<Projects.NotificationWorkerService>("notificationworkerservice")
    .WithReference(kafka);

var transactionservice = builder
    .AddProject<Projects.TransactionService>("transactionservice")
    .WithReference(accountservices)
    .WithReference(customerprofile)
    .WithReference(redis)
    .WithReference(kafka);

builder
    .AddProject<Projects.YarpApiGateWay>("yarpapigateway")
    .WithReference(transactionservice)
    .WithReference(customerprofile)
    .WithReference(redis);

builder.Build().Run();
