using JasperFx.Core;
using Marten;
using Marten.Events;
using Wolverine;
using Wolverine.ErrorHandling;
using static WolverineIssues.ExceptionHandlingRetryIssue.ExceptionHandlingIssueCommands;
using static WolverineIssues.ExceptionHandlingRetryIssue.ExceptionHandlingIssueEvents;

namespace WolverineIssues.ExceptionHandlingRetryIssue;

public static class ExceptionHandlingIssueCommands
{
    public sealed record Trigger(string Id);
}

public static class ExceptionHandlingIssueEvents
{
    public sealed record MyEvent(string Id, string Source = "ExceptionHandlingIssueEvents");
}

public sealed class MyException() : Exception("Uh oh!");

public sealed class RetryIssueHandler
{
    /// <summary>
    /// Step 1, create the initial event. 
    /// </summary>
    public static void Handle(Trigger command, IDocumentSession session)
    {
        session.Events.StartStream(command.Id, new MyEvent(command.Id));
    }

    /// <summary>
    /// Step 2, we handle the event from step 1. This will:
    /// 1. Throw an exception
    /// 2. Be retried with RetryWithCooldown, and throw an exception again
    /// 3. It SHOULD be retried with ScheduleRetry, but it will not be 
    /// </summary>
    public static void Handle(IEvent<MyEvent> @event, ILogger<RetryIssueHandler> logger)
    {
        logger.LogInformation("Handling IEvent<MyEvent>. This will throw an exception, and should be retried.");
        throw new MyException();
    }

    public static void ApplyExceptionHandling(WolverineOptions opts)
    {
        opts.OnException<MyException>()
            .RetryWithCooldown(100.Milliseconds()) // <-- this works
            .Then
            .ScheduleRetry(100.Milliseconds()); // <-- this will be unrouteable by wolverine
        
        /*
            This will show up in the logs
         
          2024-07-22T09:48:34.1294830 info: Wolverine.Runtime.WolverineRuntime[0]
                 Locally enqueuing scheduled message 0190d969-ddf5-466e-a66c-bb6f40cd51ba of type EventMyEvent
           2024-07-22T09:48:34.1341140 info: Wolverine.Runtime.WolverineRuntime[106]
                 66bc86c6-b0b2-4c5c-805d-45af883094a5: No known handler for EventMyEvent#0190d969-ddf5-466e-a66c-bb6f40cd51ba from local://replies/
         */
    }
}
