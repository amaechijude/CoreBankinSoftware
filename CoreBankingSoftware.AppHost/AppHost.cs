var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.CustomerAPI>("customerapi");

builder.Build().Run();
