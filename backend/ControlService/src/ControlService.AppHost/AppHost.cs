// Docker naming convention for this project: controlservice-<service> for containers,
// controlservice-<service>-data for volumes, so they are easy to tell apart in Docker Desktop.
const string NamePrefix = "controlservice";

var builder = DistributedApplication.CreateBuilder(args);

// Persistent containers keep running after the AppHost stops and are reused on the next start,
// which a fixed container name requires. Stop them in Docker Desktop when you are done for the day.
// The database files live in the named volume, so they survive even if the container is removed.
var postgres = builder.AddPostgres("postgres")
    .WithContainerName($"{NamePrefix}-postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume($"{NamePrefix}-postgres-data");

var database = postgres.AddDatabase("controlservice");

// Test inbox: catches every e-mail the API sends (ADR-0030). Nothing reaches real people.
var mailpit = builder.AddMailPit("mailpit")
    .WithContainerName($"{NamePrefix}-mailpit")
    .WithLifetime(ContainerLifetime.Persistent);

builder.AddProject<Projects.ControlService_API>("api")
    .WithReference(database)
    .WaitFor(database)
    .WithReference(mailpit)
    .WaitFor(mailpit);

builder.Build().Run();
