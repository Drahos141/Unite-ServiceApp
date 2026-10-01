namespace Unite.Shared.Telemetry;

/// <summary>
/// Point-in-time health/activity snapshot of a backend service, derived from the
/// events it has published, used to render the Dashboard's "connected services" overview.
/// </summary>
public sealed record ServiceStatusSnapshot(
    string ServiceName,
    DateTimeOffset LastSeenUtc,
    long TotalEventsPublished);
