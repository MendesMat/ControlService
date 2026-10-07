// Docker naming convention for this project: controlservice-<service> for containers,
// controlservice-<service>-data for volumes, so they are easy to tell apart in Docker Desktop.
const string NamePrefix = "controlservice";

var builder = DistributedApplication.CreateBuilder(args);

// A persistent container keeps running after the AppHost stops and is reused on the next start,
// which a fixed container name requires. Stop it in Docker Desktop when you are done for the day.
// The database files live in the named volume, so they survive even if the container is removed.
var postgres = builder.AddPostgres("postgres")
    .WithContainerName($"{NamePrefix}-postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume($"{NamePrefix}-postgres-data");

var database = postgres.AddDatabase("controlservice");

builder.AddProject<Projects.ControlService_API>("api", launchProfileName: "https")
    .WithReference(database)
    .WaitFor(database);

builder.Build().Run();
