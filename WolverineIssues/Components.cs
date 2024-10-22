using Marten;
using Marten.Events;
using Wolverine.Attributes;

namespace WolverineIssues;

public class Aggregate;

public sealed record MyEvent;

public sealed record AnotherEvent;

// 2 handler classes both listen to MyEvent. Both cascade a command that store a new event on the same stream.

public static class MyFirstEventHandler
{
    public record MyFirstCommand(Guid AggregateId);

    public static MyFirstCommand Handle(IEvent<MyEvent> @event)
    {
        Console.WriteLine($"{nameof(MyFirstEventHandler)}");
        return new MyFirstCommand(@event.StreamId);
    }

    public static async Task Handle(MyFirstCommand command, IDocumentSession session)
    {
        session.Events.Append(command.AggregateId, new AnotherEvent());
        await session.SaveChangesAsync();
    }
}

public static class MySecondEventHandler
{
    public record MySecondCommand(Guid AggregateId);

    public static MySecondCommand Handle(IEvent<MyEvent> @event)
    {
        Console.WriteLine($"{nameof(MySecondEventHandler)}");
        return new MySecondCommand(@event.StreamId);
    }

    public static async Task Handle(MySecondCommand command, IDocumentSession session)
    {
        session.Events.Append(command.AggregateId, new AnotherEvent());
        await session.SaveChangesAsync();
    }
}
