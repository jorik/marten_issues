using Marten;
using Marten.Events.Daemon.Resiliency;
using Weasel.Core;
using Wolverine;
using Wolverine.Marten;
using WolverineIssues;

var builder = WebApplication.CreateBuilder(args);

builder
    .Services
    .AddSwaggerGen()
    .AddEndpointsApiExplorer()
    .AddNpgsqlDataSource(builder.Configuration.GetConnectionString("wolverinedb")!);

builder.Host.UseWolverine(opts =>
{
    opts.Policies.AutoApplyTransactions();
    opts.Policies.UseDurableLocalQueues();
});

var marten = builder.Services.AddMarten(opts =>
{
    opts.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;

    opts.Projections.Errors.SkipApplyErrors = false;
    opts.Projections.Errors.SkipUnknownEvents = false;
    opts.Projections.Errors.SkipSerializationErrors = false;
});
marten
    .UseLightweightSessions()
    .ApplyAllDatabaseChangesOnStartup()
    .AddAsyncDaemon(DaemonMode.Solo)
    .UseNpgsqlDataSource()
    .IntegrateWithWolverine()
    .ProcessEventsWithWolverineHandlersInStrictOrder("Events", e =>
    {
        e.FilterIncomingEventsOnStreamType(typeof(Aggregate));
    });

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(e => e.EnableTryItOutByDefault());

app.MapPost("/ievent-sticky-handler", async (IDocumentSession session) =>
{
    session.Events.StartStream<Aggregate>(new MyEvent());
    await session.SaveChangesAsync();
});

app.Run();


