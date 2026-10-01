using Unite.Shared.Events.Orders;
using Unite.Telemetry.Service;

namespace Unite.Shared.Tests.Telemetry;

public class TelemetryStoreTests
{
    [Fact]
    public void Record_AddsEventToRecentActivity()
    {
        var store = new TelemetryStore();
        var orderCreated = new OrderCreated(Guid.NewGuid(), "Ada Lovelace", 10m);

        var recorded = store.Record(orderCreated);

        var activity = Assert.Single(store.GetRecentActivity());
        Assert.Equal(orderCreated.EventId, activity.EventId);
        Assert.Equal("Orders", activity.SourceService);
        Assert.Equal(nameof(OrderCreated), activity.EventType);
        Assert.Equal(recorded, activity);
    }

    [Fact]
    public void Record_TracksPerServiceEventCountAndLastSeen()
    {
        var store = new TelemetryStore();

        store.Record(new OrderCreated(Guid.NewGuid(), "Ada Lovelace", 10m));
        store.Record(new OrderCreated(Guid.NewGuid(), "Grace Hopper", 20m));

        var status = store.GetServiceStatus("Orders");

        Assert.Equal(2, status.TotalEventsPublished);
        Assert.NotEqual(DateTimeOffset.MinValue, status.LastSeenUtc);
    }

    [Fact]
    public void GetServiceStatus_ReturnsDefaultSnapshotForUnknownService()
    {
        var store = new TelemetryStore();

        var status = store.GetServiceStatus("Unknown");

        Assert.Equal("Unknown", status.ServiceName);
        Assert.Equal(0, status.TotalEventsPublished);
        Assert.Equal(DateTimeOffset.MinValue, status.LastSeenUtc);
    }

    [Fact]
    public void GetServiceStatuses_ReturnsOneSnapshotPerSourceService()
    {
        var store = new TelemetryStore();

        store.Record(new OrderCreated(Guid.NewGuid(), "Ada Lovelace", 10m));

        var statuses = store.GetServiceStatuses();

        var snapshot = Assert.Single(statuses);
        Assert.Equal("Orders", snapshot.ServiceName);
    }
}
