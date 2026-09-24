var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume();

var database = postgres.AddDatabase("controlservice");

var mailpit = builder.AddMailPit("mailpit");

builder.AddProject<Projects.ControlService_API>("api")
    .WithReference(database)
    .WaitFor(database)
    .WithReference(mailpit)
    .WaitFor(mailpit);

builder.Build().Run();
