namespace Unite.Shared.Events;

/// <summary>
/// Marker contract implemented by every domain/integration event that flows across
/// the message bus. Implementing this interface allows the Telemetry service to
/// discover, consume, and record every event published in the system without each
/// service needing to register a dedicated audit consumer.
/// </summary>
public interface IIntegrationEvent
{
    /// <summary>Unique identifier of this specific event occurrence.</summary>
    Guid EventId { get; }

    /// <summary>UTC timestamp the event was raised at the source service.</summary>
    DateTimeOffset OccurredAtUtc { get; }

    /// <summary>Logical name of the service that published the event (e.g. "Orders").</summary>
    string SourceService { get; }
}
