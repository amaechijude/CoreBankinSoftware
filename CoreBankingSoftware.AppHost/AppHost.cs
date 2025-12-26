using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis").WithDataVolume().WithRedisCommander();

var customerprofile = builder.AddProject<Projects.CustomerProfile>("customerprofile");
var accountservices = builder
    .AddProject<Projects.AccountServices>("accountservices")
    .WithReference(customerprofile);

var notificationworkerservice = builder.AddProject<Projects.NotificationWorkerService>(
    "notificationworkerservice"
);

var transactionservice = builder
    .AddProject<Projects.TransactionService>("transactionservice")
    .WithReference(accountservices)
    .WithReference(customerprofile);

builder
    .AddProject<Projects.YarpApiGateWay>("yarpapigateway")
    .WithReference(transactionservice)
    .WithReference(accountservices)
    .WithReference(customerprofile)
    .WithReference(notificationworkerservice)
    .WithReference(redis);

builder.Build().Run();
