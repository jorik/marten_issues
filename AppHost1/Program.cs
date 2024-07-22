using AppHost1.Containers.Postgres;
using Aspirant.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithEnvironment("POSTGRES_DB", "wolverinedb")
    .WithHealthCheck()
    .WithPgAdmin();

var db = postgres.AddDatabase("wolverinedb");

var api = builder.AddProject<Projects.WolverineIssues>("api")
    .WithReference(db)
    .WaitFor(db);

builder.Build().Run();