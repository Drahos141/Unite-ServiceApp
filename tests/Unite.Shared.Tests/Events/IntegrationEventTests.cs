using Unite.Shared.Events.Orders;

namespace Unite.Shared.Tests.Events;

public class IntegrationEventTests
{
    [Fact]
    public void IntegrationEvent_AssignsUniqueEventIdAndTimestamp()
    {
        var before = DateTimeOffset.UtcNow;
        var orderCreated = new OrderCreated(Guid.NewGuid(), "Ada Lovelace", 42.5m);
        var after = DateTimeOffset.UtcNow;

        Assert.NotEqual(Guid.Empty, orderCreated.EventId);
        Assert.InRange(orderCreated.OccurredAtUtc, before, after);
    }

    [Fact]
    public void IntegrationEvent_SetsSourceServiceFromConcreteEventType()
    {
        var orderCreated = new OrderCreated(Guid.NewGuid(), "Ada Lovelace", 42.5m);
        var orderConfirmed = new OrderConfirmed(orderCreated.OrderId);

        Assert.Equal("Orders", orderCreated.SourceService);
        Assert.Equal("Orders", orderConfirmed.SourceService);
    }

    [Fact]
    public void IntegrationEvent_EachInstanceGetsADistinctEventId()
    {
        var first = new OrderCreated(Guid.NewGuid(), "Ada Lovelace", 1m);
        var second = new OrderCreated(Guid.NewGuid(), "Ada Lovelace", 1m);

        Assert.NotEqual(first.EventId, second.EventId);
    }
}
