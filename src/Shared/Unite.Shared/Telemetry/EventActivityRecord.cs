namespace Unite.Shared.Telemetry;

/// <summary>
/// Flattened, serializable representation of an <see cref="Events.IIntegrationEvent"/>
/// used to stream event activity to the Dashboard over SignalR and to render the
/// "connected services" overview.
/// </summary>
public sealed record EventActivityRecord(
    Guid EventId,
    string SourceService,
    string EventType,
    DateTimeOffset OccurredAtUtc,
    DateTimeOffset RecordedAtUtc,
    string Payload);
