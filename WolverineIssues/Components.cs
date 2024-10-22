using Marten.Events;
using Wolverine.Attributes;

namespace WolverineIssues;

public class Aggregate;

public sealed record MyEvent;

public static class MyFirstEventHandler
{
    [StickyHandler(nameof(MyFirstEventHandler))]
    public static void Handle(IEvent<MyEvent> @event)
    {
        Console.WriteLine($"{nameof(MyFirstEventHandler)}");
    }
}

public static class MySecondEventHandler
{
    [StickyHandler(nameof(MySecondEventHandler))]
    public static void Handle(IEvent<MyEvent> @event)
    {
        Console.WriteLine($"{nameof(MySecondEventHandler)}");
    }
}
