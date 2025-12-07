var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.CustomerAPI>("customerapi");

builder.AddProject<Projects.Notification>("notification");

builder.Build().Run();
