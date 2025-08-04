var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.TownTrek>("towntrek");

builder.Build().Run();
