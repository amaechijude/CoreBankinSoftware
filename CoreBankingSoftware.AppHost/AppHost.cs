using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis");

var customerprofile = builder.AddProject<Projects.CustomerProfile>("customerprofile");
var accountservices = builder
    .AddProject<Projects.AccountServices>("accountservices")
    .WithReference(customerprofile);

var _ = builder.AddProject<Projects.NotificationWorkerService>(
    "notificationworkerservice"
);

var transactionservice = builder
    .AddProject<Projects.TransactionService>("transactionservice")
    .WithReference(accountservices)
    .WithReference(customerprofile)
    .WithReference(redis);

builder
    .AddProject<Projects.YarpApiGateWay>("yarpapigateway")
    .WaitFor(transactionservice)
    .WaitFor(accountservices)
    .WaitFor(customerprofile)
    .WaitFor(redis);

builder.Build().Run();
