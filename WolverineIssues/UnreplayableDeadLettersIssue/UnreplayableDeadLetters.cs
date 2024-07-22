using Marten;
using Marten.Events;
using static WolverineIssues.UnreplayableDeadLettersIssue.UnreplayableDeadLettersIssueCommands;
using static WolverineIssues.UnreplayableDeadLettersIssue.UnreplayableDeadLetterEvents;

namespace WolverineIssues.UnreplayableDeadLettersIssue;

public static class UnreplayableDeadLettersIssueCommands
{
    public sealed record Trigger(string Id);
}

public static class UnreplayableDeadLetterEvents
{
    public sealed record MyEvent(string Id);
}

public sealed class AnotherCustomException() : Exception("Uh oh! Now try to replay me!");

public sealed class UnreplayableDeadLettersIssueHandler
{
    /// <summary>
    /// Step 1, create the initial event. 
    /// </summary>
    public static void Handle(Trigger command, IDocumentSession session)
    {
        session.Events.StartStream(command.Id, new MyEvent(command.Id));
    }

    /// <summary>
    /// Step 2. Throw an exception without a retry policy.
    /// If you try to replay this event, it will not be routable by wolverine
    ///
    /// These logs will be seen when replaying the event:
    /// </summary>
    /*
     2024-07-22T10:01:49.6764640 info: Wolverine.RDBMS.DurabilityAgent[0]
     Issuing a command to recover 1 incoming messages from the inbox to destination local://ieventmyevent/

     2024-07-22T10:01:49.7236830 info: Wolverine.RDBMS.DurabilityAgent[206]
     Recovered 1 incoming envelopes from storage
     
     2024-07-22T10:01:49.7256430 info: Wolverine.RDBMS.DurabilityAgent[0]
     Successfully recovered 1 messages from the inbox for listener local://ieventmyevent/
     
     2024-07-22T10:01:49.7280290 info: Wolverine.Runtime.WolverineRuntime[106]
     0668fabe-6c38-4f19-b2ee-ea18388dbb27: No known handler for EventMyEvent#0190d975-6ba1-4819-8273-73751d716b3c from local://replies/
    */
    public static void Handle(IEvent<MyEvent> @event, ILogger<UnreplayableDeadLettersIssueHandler> logger)
    {
        logger.LogInformation(
            "Handling IEvent<MyEvent>. This will throw an exception. If you try replaying it be setting replayable=true in wolverine_dead_letters, the message is not actually tretried");
        throw new AnotherCustomException();
    }
}