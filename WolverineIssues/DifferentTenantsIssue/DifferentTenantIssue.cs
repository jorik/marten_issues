using Marten;
using Marten.Events;

using static WolverineIssues.DifferentTenantsIssue.DifferentTenantIssueCommands;
using static WolverineIssues.DifferentTenantsIssue.DifferentTenantIssueEvents;

namespace WolverineIssues.DifferentTenantsIssue;

public static class DifferentTenantIssueCommands
{
    public sealed record Trigger(string Id);
}

public static class DifferentTenantIssueEvents
{
    public sealed record InitialEvent(string Id);
    public sealed record SecondEvent(string Id);
}

public static class DifferentTenantIdHandler
{
    /// <summary>
    /// Step 1, create the initial event.
    ///
    /// This should have tenant_id: *DEFAULT*
    /// </summary>
    /// <param name="command"></param>
    /// <param name="session"></param>
    public static void Handle(Trigger command, IDocumentSession session)
    {
        session.Events.StartStream(command.Id, new InitialEvent(command.Id));
    }

    /// <summary>
    /// Step 2, we handle the event from step 1.
    ///
    /// This now gets stored with tenant_id: Marten
    /// </summary>
    /// <param name="event"></param>
    /// <param name="session"></param>
    public static void Handle(IEvent<InitialEvent> @event, IDocumentSession session)
    {
        session.Events.Append(@event.Data.Id, new SecondEvent(@event.Data.Id));
    }
}
