namespace Unite.Shared.Events;

/// <summary>
/// Convenience base record for integration events. Concrete events should inherit
/// from this to automatically get an <see cref="EventId"/> and <see cref="OccurredAtUtc"/>.
/// </summary>
public abstract record IntegrationEvent(string SourceService) : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();

    public DateTimeOffset OccurredAtUtc { get; init; } = DateTimeOffset.UtcNow;
}
