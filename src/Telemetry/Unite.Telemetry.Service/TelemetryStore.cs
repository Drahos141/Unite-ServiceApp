using System.Collections.Concurrent;
using System.Text.Json;
using Unite.Shared.Events;
using Unite.Shared.Telemetry;

namespace Unite.Telemetry.Service;

/// <summary>
/// In-memory store of recent integration event activity and per-service status
/// snapshots, used to answer the Dashboard's initial "connected services" overview
/// request and to feed the live SignalR stream.
/// </summary>
public sealed class TelemetryStore
{
    private const int MaxHistory = 200;

    private readonly ConcurrentQueue<EventActivityRecord> _history = new();
    private readonly ConcurrentDictionary<string, ServiceStatusSnapshot> _services = new();

    public EventActivityRecord Record(IIntegrationEvent integrationEvent)
    {
        var activity = new EventActivityRecord(
            EventId: integrationEvent.EventId,
            SourceService: integrationEvent.SourceService,
            EventType: integrationEvent.GetType().Name,
            OccurredAtUtc: integrationEvent.OccurredAtUtc,
            RecordedAtUtc: DateTimeOffset.UtcNow,
            Payload: JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType()));

        _history.Enqueue(activity);
        while (_history.Count > MaxHistory && _history.TryDequeue(out _))
        {
        }

        _services.AddOrUpdate(
            activity.SourceService,
            _ => new ServiceStatusSnapshot(activity.SourceService, activity.RecordedAtUtc, 1),
            (_, existing) => existing with
            {
                LastSeenUtc = activity.RecordedAtUtc,
                TotalEventsPublished = existing.TotalEventsPublished + 1,
            });

        return activity;
    }

    public IReadOnlyCollection<EventActivityRecord> GetRecentActivity() => _history.ToArray();

    public IReadOnlyCollection<ServiceStatusSnapshot> GetServiceStatuses() => _services.Values.ToArray();

    public ServiceStatusSnapshot GetServiceStatus(string serviceName) =>
        _services.TryGetValue(serviceName, out var status)
            ? status
            : new ServiceStatusSnapshot(serviceName, DateTimeOffset.MinValue, 0);
}
