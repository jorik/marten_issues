using Marten;
using Marten.Events;
using Marten.Events.Daemon.Resiliency;
using Microsoft.AspNetCore.Mvc;
using Weasel.Core;
using Wolverine;
using Wolverine.Marten;
using WolverineIssues.DifferentTenantsIssue;
using WolverineIssues.ExceptionHandlingRetryIssue;
using WolverineIssues.UnreplayableDeadLettersIssue;
using RetryIssueHandler = WolverineIssues.ExceptionHandlingRetryIssue.RetryIssueHandler;

var builder = WebApplication.CreateBuilder(args);

builder
    .Services
    .AddSwaggerGen()
    .AddEndpointsApiExplorer()
    .AddNpgsqlDataSource(builder.Configuration.GetConnectionString("wolverinedb")!);

builder.Host.UseWolverine(opts =>
{
    RetryIssueHandler.ApplyExceptionHandling(opts);

    opts.Policies.AutoApplyTransactions();
    opts.Policies.UseDurableLocalQueues();
});

var marten = builder.Services.AddMarten(opts =>
{
    opts.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;
    opts.Events.DatabaseSchemaName = "public";
    opts.Events.StreamIdentity = StreamIdentity.AsString;

    opts.Projections.Errors.SkipApplyErrors = false;
    opts.Projections.Errors.SkipUnknownEvents = false;
    opts.Projections.Errors.SkipSerializationErrors = false;

    opts.UseSystemTextJsonForSerialization(EnumStorage.AsString);
});
marten
    .UseLightweightSessions()
    .ApplyAllDatabaseChangesOnStartup()
    .AddAsyncDaemon(DaemonMode.Solo)
    .UseNpgsqlDataSource()
    .IntegrateWithWolverine()
    .PublishEventsToWolverine("demo.internal-events");

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(e => e.EnableTryItOutByDefault());

app.MapPost("/different-tenants", async (IMessageBus bus) =>
    {
        var streamId = Guid.NewGuid().ToString();
        await bus.InvokeAsync(new DifferentTenantIssueCommands.Trigger(streamId));

        return new { streamId };
    })
    .WithSummary("Starts the different tenant issue.");

app.MapPost("/exception-handling", async (IMessageBus bus) =>
    {
        var streamId = Guid.NewGuid().ToString();
        await bus.InvokeAsync(new ExceptionHandlingIssueCommands.Trigger(streamId));

        return new { streamId };
    })
    .WithSummary("Starts the exception handling issue.");

app.MapPost("/unreplayable-dead-letters", async (IMessageBus bus) =>
    {
        var streamId = Guid.NewGuid().ToString();
        await bus.InvokeAsync(new UnreplayableDeadLettersIssueCommands.Trigger(streamId));

        return new { streamId };
    })
    .WithSummary("Starts the unreplayable dead letters issue.");

app.Run();