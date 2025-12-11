var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.CustomerProfile>("customerapi");

builder.AddProject<Projects.Notification>("notification");

builder.Build().Run();
