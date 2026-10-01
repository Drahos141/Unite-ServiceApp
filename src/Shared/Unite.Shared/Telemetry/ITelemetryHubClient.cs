namespace Unite.Shared.Telemetry;

/// <summary>
/// Strongly-typed client contract implemented by SignalR clients (the Dashboard)
/// connecting to the Telemetry hub, used so the hub can push live updates.
/// </summary>
public interface ITelemetryHubClient
{
    /// <summary>Invoked whenever a new integration event is recorded anywhere in the system.</summary>
    Task EventRecorded(EventActivityRecord activity);

    /// <summary>Invoked whenever a connected service's status snapshot changes.</summary>
    Task ServiceStatusChanged(ServiceStatusSnapshot status);
}

/// <summary>Route constants shared by the Telemetry service and its consumers (Gateway, Dashboard).</summary>
public static class TelemetryHubRoute
{
    public const string Path = "/hubs/telemetry";
}
